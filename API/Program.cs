using API;
using API.Errors;
using API.Extensions;
using API.Helpers;
using API.Middlewares;
using Core.Entities.Identity;
using Core.ServiceHelpers.EmailSenderService;
using Infrastructure.Data.Identity;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            Log.Information("Add services to the container.");

            // configure Host with serilog
            builder.Host.ConfigureSeriLog();

            // Add services to the container.
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddLocalizationServices(builder.Configuration);
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
        
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddSwaggerDocumentation();

            var app = builder.Build();

            Log.Information("Configure the HTTP request pipeline.");

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            // Configure the HTTP request pipeline.
            app.UseSwaggerDocumentation();

            //if (app.Environment.IsDevelopment())
            //{
                  //app.UseSwagger();
                  //app.UseSwaggerUI();
            //}

            app.UseSerilogRequestLogging();

            app.UseMiddleware<MethodNotAllowedMiddleware>();
            app.UseMiddleware<NoContentMiddleware>();

            app.UseHttpsRedirection();

            app.UseCors("corsPolicy");
            
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.MapControllers();


            
            Log.Information("Migrate database, seed sata & resolve some services.");
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var identityContext = services.GetRequiredService<AppIdentityDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<AppUserRole>>();
            var sharedLocalizer = services.GetRequiredService<IStringLocalizer<SharedResource>>();

            await identityContext.Database.MigrateAsync();
            await AppIdentityDbContextSeed.SeedIdentityAsync(userManager, roleManager);
            ApiResponse.SetLocalizer(sharedLocalizer);
            LocalizerManager.SetLocalizer(sharedLocalizer);

            Log.Information("Starting the application");
            app.Run();
        }
        catch (Exception ex) 
        when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
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