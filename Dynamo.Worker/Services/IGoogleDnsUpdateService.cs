namespace Dynamo.Worker.Services;

public interface IGoogleDnsUpdateService
{
    Task UpdateAllHostnames(string ipAddress, CancellationToken cancellationToken);
}