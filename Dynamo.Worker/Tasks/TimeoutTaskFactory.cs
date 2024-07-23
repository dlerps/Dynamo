namespace Dynamo.Worker.Tasks;

public class TimeoutTaskFactory : ITimeoutTaskFactory
{
    private readonly ILogger<TimeoutTaskFactory> _logger;
    private readonly Random _random;

    public TimeoutTaskFactory(ILogger<TimeoutTaskFactory> logger)
    {
        _logger = logger;
        _random = new Random((int)DateTime.Now.Ticks);
    }

    public Task Create(
        IEnabledConfiguration hostConfiguration,
        int timeoutInMinutes,
        CancellationToken cancellationToken)
    {
        // add some random seconds to avoid overlaps
        var delay = 60 * timeoutInMinutes + _random.Next(0, 30);
        
        return Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
                
                _logger.LogInformation(
                    "{Configuration} timeout of {Seconds} seconds expired. Re-enabling configuration",
                    hostConfiguration.Identifier,
                    delay);
                
                hostConfiguration.Enabled = true;
            },
            cancellationToken);
    }
}