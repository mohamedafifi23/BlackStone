using Serilog;
using System.Diagnostics;

namespace API.Extensions
{
    public static class LoggingExtensions
    {
        public static void AddLogger(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((ctx, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                    .Enrich.WithProperty("Environment", ctx.HostingEnvironment);

            #if DEBUG
                // Used to filter out potentially bad data due debugging.
                // Very useful when doing Seq dashboards and want to remove logs under debugging session.
                loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
            #endif
            });
        }
    }
}
