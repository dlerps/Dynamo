
using Dynamo.Worker.GoogleDomains.Configuration;
using Dynamo.Worker.IpInfo;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker;

public class Worker : BackgroundService
{
    private readonly IIpInfoApi _ipInfoApi;
    private readonly GoogleDomainsOptions _googleDomainOptions;
    private readonly ILogger<Worker> _logger;

    public Worker(IIpInfoApi ipInfoApi,
        IOptions<GoogleDomainsOptions> googleDomainOptions,
        ILogger<Worker> logger)
    {
        _ipInfoApi = ipInfoApi;
        _googleDomainOptions = googleDomainOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ip = await _ipInfoApi.GetPublicIpAddress();
            _logger.LogInformation("Updating {Host} with {IpAddress}", _googleDomainOptions.Host, ip);

            await Task.Delay(10000, stoppingToken);
        }
    }
}
