using System.Text.Json.Serialization;

namespace Dynamo.Worker.Cloudflare.Model;

public record ResultInfo
{
    [JsonPropertyName("page")]
    public uint Page { get; set; }  = 0;
    
    [JsonPropertyName("per_page")]
    public uint PerPage { get; set; } = 0;
    
    [JsonPropertyName("total_pages")]
    public uint TotalPages { get; set; } = 0;
    
    [JsonPropertyName("count")]
    public uint Count { get; set; } = 0;
    
    [JsonPropertyName("total_count")]
    public uint TotalCount { get; set; } = 0;
}