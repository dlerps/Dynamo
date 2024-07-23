using System.Text.Json.Serialization;

namespace Dynamo.Worker.Cloudflare.Model;

public record CloudflareApiResult<TResult>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("result")]
    public TResult Result { get; set; } = default!;
    
    [JsonPropertyName("result_info")]
    public ResultInfo ResultInfo { get; set; } = new();
}