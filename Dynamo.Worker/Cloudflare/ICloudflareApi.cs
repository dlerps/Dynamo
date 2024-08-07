﻿using Dynamo.Worker.Cloudflare.Model;
using Refit;

namespace Dynamo.Worker.Cloudflare;

public interface ICloudflareApi
{
    [Get("/zones/{zoneId}/dns_records")]
    Task<CloudflareApiResult<IEnumerable<DnsRecordInfoDto>>?> GetDnsRecords(
        [AliasAs("zoneId")] string zoneId, 
        [Authorize] string apiToken,
        CancellationToken cancellationToken = default);
    
    [Patch("/zones/{zoneId}/dns_records/{recordId}")]
    Task<HttpResponseMessage> UpdateDnsRecord(
        [AliasAs("zoneId")] string zoneId, 
        [AliasAs("recordId")] string recordId, 
        [Body] DnsRecordInfoDto request, 
        [Authorize] string apiToken,
        CancellationToken cancellationToken = default);
}