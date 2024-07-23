namespace Dynamo.Worker.GoogleDomains.Services;

public interface IGoogleDnsUpdateService
{
    Task UpdateAllHostnames(string ipAddress, CancellationToken cancellationToken);
}