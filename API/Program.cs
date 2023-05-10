using API.Extensions;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

                var builder = WebApplication.CreateBuilder(args);

                Log.Information("Add services to the container.");

                // configure Host with serilog
                builder.Host.ConfigureSeriLog();

                // Add services to the container.
                builder.Services.AddLocalizationServices(builder.Configuration);
                builder.Services.AddControllers();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                Log.Information("Configure the HTTP request pipeline.");

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                
                app.UseSerilogRequestLogging();

                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

                app.MapControllers();

                Log.Information("Starting the application");

                app.Run();

            }
            catch (Exception ex)
            {
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger();
                }

                Log.Fatal(ex, "Application terminated unexpectedly.");
            }
            finally
            {
                Log.Information("Shut down complete.");
                Log.CloseAndFlush();
            }
        }
    }
}