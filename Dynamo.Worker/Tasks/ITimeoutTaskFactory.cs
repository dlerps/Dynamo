namespace Dynamo.Worker.Tasks;

public interface ITimeoutTaskFactory
{
    Task Create(
        IEnabledConfiguration hostConfiguration,
        int timeoutInMinutes,
        CancellationToken cancellationToken);
}