
using Dynamo.Worker.Services;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceProvider serviceProvider,
        ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Run(cancellationToken);
            await Task.Delay(10000, cancellationToken);
        }
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        var ipService = serviceScope.ServiceProvider.GetRequiredService<IIpAddressService>();
        var ipAddress = await ipService.GetIpAddress();

        if (!await ipService.HasIpAddressChanged(ipAddress))
        {
            _logger.LogDebug("IP address {IpAddress} has not changed. No updates necessary...", ipAddress);
            return;
        }
        
        _logger.LogInformation("Updating Google Domains DNS records...");
        
        var googleDnsUpdaterService = serviceScope.ServiceProvider.GetRequiredService<IGoogleDnsUpdateService>();
        await googleDnsUpdaterService.UpdateAllHostnames(ipAddress, cancellationToken);
    }
}
