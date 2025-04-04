namespace Jumia_Clone.Configuration
{
    public static class CorsConfiguration
    {
        public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                        //.WithOrigins(
                        //    // Allow Angular development server
                        //    "http://localhost:4200",
                        //    // Add production URLs as needed
                        //    configuration["ClientUrl"] ?? "http://localhost:4200"
                        //)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}
