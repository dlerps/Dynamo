using Dynamo.Worker.GoogleDomains.Configuration;

namespace Dynamo.Worker.Tasks;

public interface ITimeoutTaskFactory
{
    Task Create(
        GoogleDomainsHostConfiguration hostConfiguration,
        int timeoutInMinutes,
        CancellationToken cancellationToken);
}