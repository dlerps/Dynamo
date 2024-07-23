using Dynamo.Worker.Configuration;
using Dynamo.Worker.GoogleDomains.Configuration;
using Dynamo.Worker.GoogleDomains.Model;
using Dynamo.Worker.Tasks;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker.GoogleDomains.Services;

public class GoogleDnsUpdateService : IGoogleDnsUpdateService
{
    private readonly IGoogleDomainsApi _googleDomainsApi;
    private readonly IGoogleDomainsResponseInterpreter _responseInterpreter;
    private readonly GoogleDomainsOptions _googleDomainsOptions;
    private readonly ILongRunningTaskService _longRunningTaskService;
    private readonly ITimeoutTaskFactory _timeoutTaskFactory;
    private readonly ILogger<GoogleDnsUpdateService> _logger;
    private readonly string _userAgent;

    public GoogleDnsUpdateService(
        IGoogleDomainsApi googleDomainsApi,
        IGoogleDomainsResponseInterpreter responseInterpreter,
        IOptions<GoogleDomainsOptions> googleDomainsOptions,
        ILongRunningTaskService longRunningTaskService,
        ITimeoutTaskFactory timeoutTaskFactory,
        IOptions<DynamoOptions> dynamoOptions,
        ILogger<GoogleDnsUpdateService> logger)
    {
        _googleDomainsApi = googleDomainsApi;
        _logger = logger;
        _googleDomainsOptions = googleDomainsOptions.Value;
        _responseInterpreter = responseInterpreter;
        _longRunningTaskService = longRunningTaskService;
        _timeoutTaskFactory = timeoutTaskFactory;
        _userAgent = dynamoOptions.Value.UserAgentHeader;
    }

    public async Task UpdateAllHostnames(string ipAddress, CancellationToken cancellationToken)
    {
        if (!_googleDomainsOptions.Enabled)
        {
            _logger.LogInformation("Google Domains hostname update is disabled. Skipping...");
            return;
        }
        
        foreach (var hostConfiguration in _googleDomainsOptions.Hosts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await UpdateHostname(hostConfiguration, ipAddress, cancellationToken);
        }
    }

    private async Task UpdateHostname(
        GoogleDomainsHostConfiguration hostConfiguration,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        if (!hostConfiguration.Enabled)
        {
            _logger.LogInformation("Hostname update for {Hostname} is disabled. Skipping...",
                hostConfiguration.Hostname);
            return;
        }

        _logger.LogInformation("Attempting Google Domains hostname update for {Hostname}", hostConfiguration.Hostname);

        var response = await _googleDomainsApi.PostDynamicHostnameIpUpdate(
            hostConfiguration.Hostname,
            ipAddress,
            hostConfiguration.BasicAuthToken);

        var responseInterpretation = _responseInterpreter.InterpretResponseString(response);
        var handleTask = responseInterpretation switch
        {
            GoogleDomainsResponse.Good => HandleGoodResponse(hostConfiguration.Hostname, ipAddress),
            GoogleDomainsResponse.IpAlreadySet => HandleNoChangeResponse(hostConfiguration, ipAddress, cancellationToken),
            GoogleDomainsResponse.TemporaryProblem => HandleTemporaryProblemResponse(hostConfiguration, cancellationToken),
            _ => HandleErrorResponse(responseInterpretation, hostConfiguration),
        };

        if (!handleTask.IsCompleted)
            _longRunningTaskService.Add(handleTask);
        
        // wait for 5 secs in case there are following update calls
        await Task.Delay(5000, cancellationToken);
    }

    private Task HandleGoodResponse(string hostname, string ipAddress)
    {
        _logger.LogInformation(
            "The response indicated that the DNS record of {Hostname} was successfully updated to {IpAddress}",
            hostname,
            ipAddress);

        return Task.CompletedTask;
    }

    private Task HandleNoChangeResponse(
        GoogleDomainsHostConfiguration hostConfiguration,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "The response indicated that the DNS record of {Hostname} was already set to {IpAddress}. Waiting for 1 hour before retrying",
            hostConfiguration.Hostname,
            ipAddress);

        hostConfiguration.Enabled = false;

        return _timeoutTaskFactory.Create(hostConfiguration, 60, cancellationToken);
    }

    private Task HandleTemporaryProblemResponse(
        GoogleDomainsHostConfiguration hostConfiguration,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "The response indicated that the Google Domains server is having problems. Waiting 10 minutes until retrying");

        hostConfiguration.Enabled = false;

        return _timeoutTaskFactory.Create(hostConfiguration, 10, cancellationToken);
    }

    private Task HandleErrorResponse(
        GoogleDomainsResponse response,
        GoogleDomainsHostConfiguration hostConfiguration)
    {
        hostConfiguration.Enabled = false;

        _logger.LogError("Response {Response} is indicating an error", response.ToString());

        if (response == GoogleDomainsResponse.Abuse)
            _logger.LogCritical("Requests for {Hostname} have been blocked for API abuse", hostConfiguration.Hostname);
        if (response == GoogleDomainsResponse.AuthFailed)
            _logger.LogCritical(
                "The response indicates a failed authentication for {Hostname}. Correct your settings and retry",
                hostConfiguration.Hostname);
        if (response == GoogleDomainsResponse.ConflictingRecords)
            _logger.LogCritical(
                "The response indicates conflicting A/AAAA records for {Hostname}. Check your DNS settings and retry",
                hostConfiguration.Hostname);
        if (response == GoogleDomainsResponse.InvalidHost)
            _logger.LogCritical(
                "The response indicates that {Hostname} is an invalid hostname. Correct your settings and retry",
                hostConfiguration.Hostname);
        if (response == GoogleDomainsResponse.NonFullyQualifiedHostname)
            _logger.LogCritical(
                "The response indicates that {Hostname} is not a fully qualified hostname. Correct your settings and retry",
                hostConfiguration.Hostname);
        if (response == GoogleDomainsResponse.BannedUserAgent)
            _logger.LogCritical(
                "The response was rejected because {UserAgent} is invalid or banned. Change your settings and retry",
                _userAgent);

        _logger.LogWarning("Disabled host config for {Hostname}", hostConfiguration.Hostname);

        return Task.CompletedTask;
    }
}