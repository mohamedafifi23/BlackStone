using API.Extensions;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    });
                builder.Services.AddApplicationServices(builder.Configuration);

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

                app.UseCors("corsPolicy");
                
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