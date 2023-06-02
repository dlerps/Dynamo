using System.Text;

namespace Dynamo.Worker.GoogleDomains.Configuration;

public class GoogleDomainsHostConfiguration
{
    public string Hostname { get; set; } = String.Empty;
    public string Username { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    
    public bool Enabled { get; set; } = true;

    public string BasicAuthToken
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
}