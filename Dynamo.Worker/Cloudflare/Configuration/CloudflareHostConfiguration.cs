using Dynamo.Worker.Cloudflare.Model;
using Dynamo.Worker.Tasks;

namespace Dynamo.Worker.Cloudflare.Configuration;

public class CloudflareHostConfiguration : IEnabledConfiguration
{
    public string Hostname { get; set; } = String.Empty;
    
    public string ZoneId { get; set; } = String.Empty;

    public DnsRecordType RecordType { get; set; } = DnsRecordType.A;

    public uint? Ttl { get; set; }

    public bool? Proxied { get; set; }
    
    public bool Enabled { get; set; } = true;
    
    public string Identifier => Hostname;
}