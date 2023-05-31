using Refit;

namespace Dynamo.Worker.IpInfo;

public interface IIpInfoApi
{
    [Get("/ip")]
    Task<string> GetPublicIpAddress();
}