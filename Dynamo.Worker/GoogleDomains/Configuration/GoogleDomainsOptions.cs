namespace Dynamo.Worker.GoogleDomains.Configuration;

public class GoogleDomainsOptions
{
    public string ApiAddress { get; set; } = String.Empty;
    public GoogleDomainsHostConfiguration[] Hosts { get; set; } = Array.Empty<GoogleDomainsHostConfiguration>();
}