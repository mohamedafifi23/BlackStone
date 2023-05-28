using API.Helpers.CustomTokenProviders;
using Core.Entities.Identity;
using Core.ServiceHelpers.EmailSenderService;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("IdentityConnection"));
            });

            services.AddIdentityCore<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
            })
                .AddRoles<AppUserRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider<AppUser>>("emailconfirmation")
                .AddSignInManager<SignInManager<AppUser>>();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
                           options.TokenLifespan = TimeSpan.FromHours(2));
            services.Configure<EmailConfirmationTokenProviderOptions>(options =>
                           options.TokenLifespan = TimeSpan.FromDays(1));

            services.AddSingleton< EmailConfiguration>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Token:Key"])),
                ValidateIssuer = true,
                ValidIssuer = config["Token:Issuer"],
                ValidateAudience = false,
                ValidateLifetime = true,
                //otherwise it will add 5 mins to the expiration time. Token will be valid for extra 5 mins greater than it should be
                ClockSkew = TimeSpan.FromSeconds(0)
            };
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                });

            services.AddAuthorization();    

            return services;    
        }
    }
}
