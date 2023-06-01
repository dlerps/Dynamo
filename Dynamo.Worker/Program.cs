using System.Net.Http.Headers;
using Dynamo.Worker;
using Dynamo.Worker.Configuration;
using Dynamo.Worker.GoogleDomains.Configuration;
using Dynamo.Worker.IpInfo;
using Dynamo.Worker.IpInfo.Configuration;
using Dynamo.Worker.Services;
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
            .Configure<DynamoOptions>(context.Configuration.GetSection("Dynamo"))
            .AddSingleton<IIpAddressService, IpAddressService>()
            .AddTransient<IHttpClientConfigurator, HttpClientConfigurator>()
            .AddHostedService<Worker>();

        services.AddRefitClient<IIpInfoApi>()
            .ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                var configurator = serviceProvider.GetRequiredService<IHttpClientConfigurator>();
                var ipInfoOpt = serviceProvider.GetRequiredService<IOptions<IpInfoOptions>>();

                httpClient.BaseAddress = new Uri(ipInfoOpt.Value.ApiAddress);
                configurator.Configure(httpClient);
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