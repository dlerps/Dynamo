
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            await Run();
            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task Run()
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
        await googleDnsUpdaterService.UpdateAllHostnames(ipAddress);
    }
}
