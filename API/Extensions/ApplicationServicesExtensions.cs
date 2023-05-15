using API.Errors;
using Core.IServices;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            string? allowedHosts = configuration.GetSection("AllowedHosts").Value;
            allowedHosts = allowedHosts ?? "*";

            services.AddCors(options =>
            {
                options.AddPolicy(name: "corsPolicy", policy =>
                {
                    policy.WithOrigins(allowedHosts).AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddScoped<ITokenService, TokenService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                    .Where(e => e.Value.Errors.Count() > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage).ToArray();

                    var errorResponse = new ApiValidationErrorResponse { Errors = errors };

                    return new BadRequestObjectResult(errorResponse);
                };
            });

            return services;
        }
    }
}
