using System.Text;
using Dynamo.Worker.Configuration;
using Dynamo.Worker.IpInfo;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker.Services;

public class IpAddressService : IIpAddressService
{
    private readonly IIpInfoApi _api;
    private readonly DynamoOptions _dynamoOptions;
    private readonly ILogger<IpAddressService> _logger;

    public IpAddressService(IIpInfoApi api, IOptions<DynamoOptions> dynamoOptions, ILogger<IpAddressService> logger)
    {
        _api = api;
        _logger = logger;
        _dynamoOptions = dynamoOptions.Value;
    }
    
    public Task<string> GetIpAddress()
    {
        return _api.GetPublicIpAddress();
    }
    
    public async Task<bool> HasIpAddressChanged(string ipAddress)
    {
        if (String.IsNullOrEmpty(ipAddress))
            throw new ArgumentException("Must not be empty", nameof(ipAddress));
        
        var previousIpAddress = File.Exists(_dynamoOptions.IpCacheFile)
            ? await File.ReadAllTextAsync(_dynamoOptions.IpCacheFile, Encoding.UTF8)
            : "0.0.0.0";

        var changed = previousIpAddress != ipAddress;
        if (changed)
        {
            _logger.LogInformation("IP address change detected {Previous} --> {Current}", previousIpAddress, ipAddress);
            await File.WriteAllTextAsync(_dynamoOptions.IpCacheFile, ipAddress, Encoding.UTF8);
        }

        return changed;
    }
    
}