namespace Dynamo.Worker.Services;

public interface IIpAddressService
{
    Task<string> GetIpAddress();
    Task<bool> HasIpAddressChanged(string ipAddress);
}