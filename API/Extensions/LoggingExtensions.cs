using Serilog;
using System.Diagnostics;

namespace API.Extensions
{
    public static class LoggingExtensions
    {
        public static IHostBuilder ConfigureSeriLog(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((ctx, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(ctx.Configuration);
                   
            #if DEBUG
                // Used to filter out potentially bad data due debugging.
                // Very useful when doing Seq dashboards and want to remove logs under debugging session.
                loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
            #endif
            });

            return hostBuilder;
        }
    }
}
