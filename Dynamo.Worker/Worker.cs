
using Dynamo.Worker.GoogleDomains.Configuration;
using Dynamo.Worker.IpInfo;
using Dynamo.Worker.Services;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker;

public class Worker : BackgroundService
{
    private readonly IIpAddressService _ipAddressService;
    private readonly GoogleDomainsOptions _googleDomainOptions;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IIpAddressService ipAddressService,
        IOptions<GoogleDomainsOptions> googleDomainOptions,
        ILogger<Worker> logger)
    {
        _ipAddressService = ipAddressService;
        _googleDomainOptions = googleDomainOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ip = await _ipAddressService.GetIpAddress();
            
            if (await _ipAddressService.HasIpAddressChanged(ip))
                _logger.LogInformation("Updating {Host} with {IpAddress}", _googleDomainOptions.Host, ip);
            else
                _logger.LogDebug("{IpAddress} remains unchanged", ip);

            await Task.Delay(10000, stoppingToken);
        }
    }
}
