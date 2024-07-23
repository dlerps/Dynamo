using Dynamo.Worker.Configuration;
using Dynamo.Worker.GoogleDomains.Services;
using Dynamo.Worker.Services;
using Dynamo.Worker.Tasks;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILongRunningTaskService _longRunningTaskService;
    private readonly DynamoOptions _dynamoOptions;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceProvider serviceProvider,
        ILongRunningTaskService longRunningTaskService,
        IOptions<DynamoOptions> dynamoOptions,
        ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _longRunningTaskService = longRunningTaskService;
        _dynamoOptions = dynamoOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var timeoutTimeSpan = TimeSpan.FromMinutes(_dynamoOptions.TimeoutInMinutes);
        
        while (true)
        {
            try
            {
                await Run(cancellationToken);

                _logger.LogDebug("Pausing for {Timeout}m", _dynamoOptions.TimeoutInMinutes);
                await Task.Delay(timeoutTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await _longRunningTaskService.DisposeAsync();
                _logger.LogDebug("Disposed long running tasks");
                
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception. Continue worker loop");
            }
        }
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        var ipService = serviceScope.ServiceProvider.GetRequiredService<IIpAddressService>();
        var ipAddress = await ipService.GetIpAddress();

        if (!await ipService.HasIpAddressChanged(ipAddress))
        {
            _logger.LogDebug("IP address {IpAddress} has not changed. No updates necessary", ipAddress);
            return;
        }
        
        _logger.LogInformation("Updating Google Domains DNS records...");
        
        var googleDnsUpdaterService = serviceScope.ServiceProvider.GetRequiredService<IGoogleDnsUpdateService>();
        await googleDnsUpdaterService.UpdateAllHostnames(ipAddress, cancellationToken);
    }
}
