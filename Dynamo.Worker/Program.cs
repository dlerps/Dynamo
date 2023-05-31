using Dynamo.Worker;
using Dynamo.Worker.GoogleDomains.Configuration;
using Dynamo.Worker.Http.Configuration;
using Dynamo.Worker.IpInfo;
using Dynamo.Worker.IpInfo.Configuration;
using Microsoft.Extensions.Options;
using Refit;
using Serilog;
using LoggerFactory = Dynamo.Worker.Logging.LoggerFactory;

Log.Logger = LoggerFactory.CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, builder) =>
    {
        builder
            .AddJsonFile("appsettings.json", false, false)
            .AddJsonFile($"appsettings.{context.HostingEnvironment}.json", true, false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services
            .AddLogging(log =>
            {
                log.ClearProviders();
                log.AddSerilog(Log.Logger, true);
            })
            .Configure<GoogleDomainsOptions>(context.Configuration.GetSection("GoogleDomains"))
            .Configure<IpInfoOptions>(context.Configuration.GetSection("IpInfo"))
            .Configure<HttpClientOptions>(context.Configuration.GetSection("HttpClient"))
            .AddHostedService<Worker>();

        services.AddRefitClient<IIpInfoApi>()
            .ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                var ipInfoOpt = serviceProvider.GetRequiredService<IOptions<IpInfoOptions>>();
                var httpClientOpt = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>();

                httpClient.BaseAddress = new Uri(ipInfoOpt.Value.ApiAddress);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", httpClientOpt.Value.UserAgentHeader);
            });
    })
    .Build();

try
{
    await host.RunAsync();
}
catch (OperationCanceledException)
{
    Log.Logger.Information("Shutting down...");
}
catch (Exception e)
{
    Log.Logger.Fatal(e, "Unexpected application shutdown");
}