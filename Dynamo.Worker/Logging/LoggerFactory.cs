using Serilog;
using Serilog.Core;
using Serilog.Exceptions;

namespace Dynamo.Worker.Logging;

public static class LoggerFactory
{
    public static Logger CreateLogger()
    {
        return new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .CreateLogger();
    }
}