using System.Net;
using Dynamo.Worker.Cloudflare.Configuration;
using Dynamo.Worker.Cloudflare.Exceptions;
using Dynamo.Worker.Cloudflare.Model;
using Dynamo.Worker.Tasks;
using Microsoft.Extensions.Options;
using Refit;

namespace Dynamo.Worker.Cloudflare.Services;

public class CloudflareDnsUpdateService : IICloudflareDnsUpdateService
{
    private const int DefaultTimeoutInMinutes = 15;

    private readonly ICloudflareApi _cloudflareApi;

    private readonly ITimeoutTaskFactory _timeoutTaskFactory;

    private readonly ILogger<CloudflareDnsUpdateService> _logger;

    private readonly CloudflareOptions _cloudflareOptions;

    public CloudflareDnsUpdateService(
        ICloudflareApi cloudflareApi,
        ITimeoutTaskFactory timeoutTaskFactory,
        IOptions<CloudflareOptions> cloudflareHostConfiguration,
        ILogger<CloudflareDnsUpdateService> logger)
    {
        _cloudflareApi = cloudflareApi;
        _timeoutTaskFactory = timeoutTaskFactory;
        _logger = logger;
        _cloudflareOptions = cloudflareHostConfiguration.Value;

        if (String.IsNullOrWhiteSpace(_cloudflareOptions.ApiToken))
            throw new NoSuchApiTokenException();
    }

    public async Task UpdateAllHostnames(string ipAddress, CancellationToken cancellationToken)
    {
        if (!_cloudflareOptions.Enabled)
        {
            _logger.LogInformation("Cloudflare hostname update is disabled. Skipping...");
            return;
        }
        
        var updateTasks = _cloudflareOptions.Hosts
            .Select(host => UpdateHostname(host, ipAddress, cancellationToken))
            .ToList();

        await Task.WhenAll(updateTasks);
    }

    private async Task UpdateHostname(
        CloudflareHostConfiguration host,
        string ipAddress,
        CancellationToken cancellationToken)
    {
        if (!host.Enabled)
        {
            _logger.LogInformation("Hostname update for {Hostname} is disabled. Skipping...", host.Hostname);
            return;
        }
        
        cancellationToken.ThrowIfCancellationRequested();

        var dnsRecords = await _cloudflareApi.GetDnsRecords(
            host.ZoneId,
            _cloudflareOptions.ApiToken,
            cancellationToken);

        if (dnsRecords is null || !dnsRecords.Result.Any())
        {
            _logger.LogError("No DNS records found for zone {ZoneId}", host.ZoneId);
            await _timeoutTaskFactory.Create(host, 5, cancellationToken);
            return;
        }

        var dnsRecord = dnsRecords.Result
            .FirstOrDefault(dns => dns.Name == host.Hostname && dns.RecordType == host.RecordType);

        if (dnsRecord is null)
        {
            _logger.LogError("No DNS record found for hostname {Hostname}", host.Hostname);
            await _timeoutTaskFactory.Create(host, 60, cancellationToken);
            return;
        }

        if (dnsRecord.Content == ipAddress)
        {
            _logger.LogDebug("DNS record for {Hostname} is already set to {IpAddress}. No action required...",
                host.Hostname,
                ipAddress
            );

            return;
        }

        _logger.LogInformation("Attempting Cloudflare hostname update for {Hostname}", host.Hostname);

        await UpdateHostRecord(host, ipAddress, dnsRecord, cancellationToken);
    }

    private async Task UpdateHostRecord(
        CloudflareHostConfiguration host,
        string ipAddress,
        DnsRecordInfoDto dnsRecord,
        CancellationToken cancellationToken)
    {
        var request = new DnsRecordInfoDto
        {
            Content = ipAddress,
            Name = host.Hostname,
            RecordType = dnsRecord.RecordType,
            Id = dnsRecord.Id
        };

        if (host.Ttl.HasValue)
            request.Ttl = host.Ttl.Value;
        
        if (dnsRecord.Proxiable == true && host.Proxied.HasValue)
            request.Proxied = host.Proxied.Value;

        try
        {
            var updatedDnsRecord = await _cloudflareApi.UpdateDnsRecord(
                host.ZoneId,
                dnsRecord.Id,
                request,
                _cloudflareOptions.ApiToken,
                cancellationToken
            );

            if (updatedDnsRecord is null || !updatedDnsRecord.Success)
            {
                _logger.LogError("Failed to update DNS record for {Hostname}", host.Hostname);
                await _timeoutTaskFactory.Create(host, DefaultTimeoutInMinutes, cancellationToken);
                return;
            }

            _logger.LogInformation("DNS record for {Hostname} was successfully updated to {IpAddress}",
                host.Hostname,
                ipAddress
            );
        }
        catch (ApiException ae)
        {
            if (ae.StatusCode != HttpStatusCode.TooManyRequests)
                throw;
            
            _logger.LogWarning("API rate limit exceeded. Waiting for 15 minutes before retrying...");
            await _timeoutTaskFactory.Create(host, DefaultTimeoutInMinutes, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update DNS record for {Hostname}", host.Hostname);
            await _timeoutTaskFactory.Create(host, 60, cancellationToken);
        }
    }
}