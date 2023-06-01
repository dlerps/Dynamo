using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Dynamo.Worker.Configuration;

public class HttpClientConfigurator : IHttpClientConfigurator
{
    private readonly DynamoOptions _dynamoOptions;
    private readonly ILogger<HttpClientConfigurator> _logger;

    public HttpClientConfigurator(IOptions<DynamoOptions> dynamoOptions, ILogger<HttpClientConfigurator> logger)
    {
        _dynamoOptions = dynamoOptions.Value;
        _logger = logger;
    }

    public void Configure(HttpClient httpClient)
    {
        var userAgentHeader = ProductInfoHeaderValue.Parse(_dynamoOptions.UserAgentHeader);
        
        _logger.LogDebug("Using {@UserAgent} as user-agent header", userAgentHeader);

        httpClient.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
    }
}