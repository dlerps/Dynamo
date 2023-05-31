namespace Dynamo.Worker.Configuration;

public class DynamoOptions
{
    public string UserAgentHeader { get; set; } = String.Empty;
    public string IpCacheFile { get; set; } = "ip.txt";
}