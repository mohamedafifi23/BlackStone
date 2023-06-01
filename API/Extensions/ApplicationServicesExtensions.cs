using API.Errors;
using Core;
using Core.IServices;
using Core.ServiceHelpers.EmailSenderService;
using Core.ServiceHelpers.PaymentService;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("AppConnection"));
            });

            string? allowedHosts = configuration.GetSection("AllowedHosts").Value;
            allowedHosts = allowedHosts ?? "*";

            services.AddCors(options =>
            {
                options.AddPolicy(name: "corsPolicy", policy =>
                {
                    policy.WithOrigins(allowedHosts).AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddScoped<IAppUserTokenService, AppUserTokenService>();

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

            services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
            services.AddScoped<IEmailSenderService, EmailSenderService>();

            var paymobConfig = configuration.GetSection("PaymobConfiguration")
                .Get<PaymobConfiguration>();
            services.AddSingleton(paymobConfig);
            services.AddHttpClient();
            services.AddScoped<IPaymentService, PaymentService>();            

            services.AddScoped<IOtpService, OtpService>();

            services.AddScoped<IUniOfWork, UnitOfWork>();

            return services;
        }
    }
}
