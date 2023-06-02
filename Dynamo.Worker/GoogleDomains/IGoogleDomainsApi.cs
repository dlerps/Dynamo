using Refit;

namespace Dynamo.Worker.GoogleDomains;

public interface IGoogleDomainsApi
{
    [Post("/update")]
    Task<string> PostDynamicHostnameIpUpdate([AliasAs("hostname")] string hostname,
        [AliasAs("myip")] string ipAddress,
        [Authorize("Basic")] string base64AuthToken);
}