using Dynamo.Worker.GoogleDomains.Configuration;

namespace Dynamo.Worker.Tasks;

public class TimeoutTaskFactory : ITimeoutTaskFactory
{
    private readonly ILogger<TimeoutTaskFactory> _logger;

    public TimeoutTaskFactory(ILogger<TimeoutTaskFactory> logger)
    {
        _logger = logger;
    }

    public Task Create(
        GoogleDomainsHostConfiguration hostConfiguration,
        int timeoutInMinutes,
        CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(timeoutInMinutes), cancellationToken);
                
                _logger.LogInformation(
                    "{Hostname} timeout of {Minutes} minutes expired. Re-enabiling configuration",
                    hostConfiguration.Hostname,
                    timeoutInMinutes);
                
                hostConfiguration.Enabled = true;
            },
            cancellationToken);
    }
}