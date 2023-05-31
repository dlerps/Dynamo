using Dynamo.Worker;
using Dynamo.Worker.Configuration;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .CreateLogger();

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
            .Configure<GoogleDomainOptions>(context.Configuration.GetSection("GoogleDomains"))
            .AddHostedService<Worker>();
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