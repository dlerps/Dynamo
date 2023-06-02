using Dynamo.Worker;
using Dynamo.Worker.Configuration;
using Dynamo.Worker.GoogleDomains;
using Dynamo.Worker.GoogleDomains.Configuration;
using Dynamo.Worker.IpInfo;
using Dynamo.Worker.IpInfo.Configuration;
using Dynamo.Worker.Services;
using Dynamo.Worker.Tasks;
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
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        Log.Logger.Information("Running in {Environment}", context.HostingEnvironment.EnvironmentName);
        
        services
            .AddLogging(log =>
            {
                log.ClearProviders();
                log.AddSerilog(Log.Logger, true);
            })
            .Configure<GoogleDomainsOptions>(context.Configuration.GetSection("GoogleDomains"))
            .Configure<IpInfoOptions>(context.Configuration.GetSection("IpInfo"))
            .Configure<DynamoOptions>(context.Configuration.GetSection("Dynamo"))
            .AddSingleton<ILongRunningTaskService, LongRunningTaskService>()
            .AddScoped<ITimeoutTaskFactory, TimeoutTaskFactory>()
            .AddScoped<IIpAddressService, IpAddressService>()
            .AddScoped<IGoogleDnsUpdateService, GoogleDnsUpdateService>()
            .AddScoped<IGoogleDomainsResponseInterpreter, GoogleDomainsResponseInterpreter>()
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
        
        services.AddRefitClient<IGoogleDomainsApi>()
            .ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                var configurator = serviceProvider.GetRequiredService<IHttpClientConfigurator>();
                var googleDomainsOpt = serviceProvider.GetRequiredService<IOptions<GoogleDomainsOptions>>();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
             
                logger.LogInformation("Google Domains API address: {ApiAddress}", googleDomainsOpt.Value.ApiAddress);
                
                httpClient.BaseAddress = new Uri(googleDomainsOpt.Value.ApiAddress);
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