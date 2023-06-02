namespace Dynamo.Worker.Tasks;

public interface ILongRunningTaskService : IAsyncDisposable
{
    void Add(Task task);
}