namespace Dynamo.Worker.Cloudflare.Services;

public interface IICloudflareDnsUpdateService
{
    Task UpdateAllHostnames(string ipAddress, CancellationToken cancellationToken);
}