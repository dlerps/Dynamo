using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace Dynamo.Worker.Logging;

public static class LoggerFactory
{
    public static Logger CreateLogger()
    {
        var envDebug = Environment.GetEnvironmentVariable("Verbose");
        var envJsonFormat = Environment.GetEnvironmentVariable("JsonLogFormat");

        var config = new LoggerConfiguration();

        if (envDebug != null && envDebug.Equals("true", StringComparison.OrdinalIgnoreCase))
            config = config.MinimumLevel.Debug();
        else
            config = config.MinimumLevel.Information();

        if (envJsonFormat != null && envJsonFormat.Equals("true", StringComparison.OrdinalIgnoreCase))
            config = config.WriteTo.Console(new CompactJsonFormatter());
        else
            config = config.WriteTo.Console();

        return config
            .WriteTo.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .CreateLogger();
    }
}