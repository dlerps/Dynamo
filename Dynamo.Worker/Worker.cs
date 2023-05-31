using Dynamo.Worker.Configuration;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker;

public class Worker : BackgroundService
{
    private readonly GoogleDomainOptions _googleDomainOptions;
    private readonly ILogger<Worker> _logger;

    public Worker(IOptions<GoogleDomainOptions> googleDomainOptions, ILogger<Worker> logger)
    {
        _googleDomainOptions = googleDomainOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            _logger.LogInformation("Updating {Host} with new IP!!", _googleDomainOptions.Host);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
