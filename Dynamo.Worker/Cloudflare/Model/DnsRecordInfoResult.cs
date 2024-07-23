using System.Text.Json.Serialization;

namespace Dynamo.Worker.Cloudflare.Model;

public record DnsRecordInfoResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("result")]
    public IEnumerable<DnsRecordInfoDto> Result { get; set; } = [];
    
    [JsonPropertyName("result_info")]
    public ResultInfo ResultInfo { get; set; } = new();
}