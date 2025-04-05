using Jumia_Clone.Data;
using Jumia_Clone.Repositories.Implementation;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Configuration
{
    public static class GeneralConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add controllers
            services.AddControllers();

            // Add OpenAPI/Swagger
            services.AddOpenApi();

            // Configure CORS
            services.ConfigureCors(configuration);

            // Configure JWT
            services.ConfigureJwt(configuration);

            // Register Services
            RegisterServices(services);

            // Register Repositories
            RegisterRepositories(services);

            // Configure Database
            ConfigureDatabase(services, configuration);

            return services;
        }

        public static WebApplication ConfigureMiddleware(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(op => op.SwaggerEndpoint("/openapi/v1.json", "v1"));

            }

            app.UseHttpsRedirection();

            // Use CORS
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // JWT Service
            services.AddScoped<JwtService>();

            // Add other services here as your project grows
            // Example: services.AddScoped<IEmailService, EmailService>();
            // Example: services.AddScoped<IFileStorageService, FileStorageService>();
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // User repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Auth repository
            services.AddScoped<IAuthRepository, AuthRepository>();

            // Add other repositories here as your project grows
            // Example: services.AddScoped<IProductRepository, ProductRepository>();
            // Example: services.AddScoped<IOrderRepository, OrderRepository>();
        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("hagerSqlCon")));
        }
    }
}