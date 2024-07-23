namespace Dynamo.Worker.Cloudflare.Condiguration;

public class CloudflareHostConfiguration
{
    public string Hostname { get; set; } = String.Empty;
    
    public string ZoneId { get; set; } = String.Empty;
    
    public string? ApiToken { get; set; }
}