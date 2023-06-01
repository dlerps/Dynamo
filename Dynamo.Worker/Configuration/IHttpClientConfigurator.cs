namespace Dynamo.Worker.Configuration;

public interface IHttpClientConfigurator
{
    void Configure(HttpClient httpClient);
}