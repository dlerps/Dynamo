namespace Dynamo.Worker.Cloudflare.Configuration;

public class CloudflareOptions
{
    public IEnumerable<CloudflareHostConfiguration> Hosts { get; set; } = [];
    
    public string ApiToken { get; set; } = String.Empty;
    
    public bool Enabled { get; set; } = true;

    public string ApiAddress { get; set; } = "https://api.cloudflare.com/client/v4";
}