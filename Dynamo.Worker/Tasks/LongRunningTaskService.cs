namespace Dynamo.Worker.Tasks;

public class LongRunningTaskService : ILongRunningTaskService
{
    private readonly List<Task> _tasks;

    public LongRunningTaskService()
    {
        _tasks = new List<Task>();
    }

    public void Add(Task task)
    {
        _tasks.Add(task);
    }

    public async ValueTask DisposeAsync()
    {
        if (_tasks.Any())
            await Task.WhenAll(_tasks);
    }
}