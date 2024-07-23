using System.Text.Json.Serialization;

namespace Dynamo.Worker.Cloudflare.Model;

public record DnsRecordInfoDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = String.Empty;
    
    [JsonPropertyName("zone_id")]
    public string ZoneId { get; set; } = String.Empty;
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
    
    [JsonPropertyName("type")]
    public DnsRecordType? RecordType { get; set; }
    
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("proxiable")]
    public bool? Proxiable { get; set; }
    
    [JsonPropertyName("proxied")]
    public bool? Proxied{ get; set; }
    
    [JsonPropertyName("locked")]
    public bool? Locked { get; set; }
    
    [JsonPropertyName("ttl")]
    public uint? Ttl { get; set; }
    
    [JsonPropertyName("created_on")]
    public DateTimeOffset? CreatedOn { get; set; }
    
    [JsonPropertyName("modified_on")]
    public DateTimeOffset? ModifiedOn { get; set; }
}