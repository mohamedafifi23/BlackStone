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

            return services;
        }
    }
}
