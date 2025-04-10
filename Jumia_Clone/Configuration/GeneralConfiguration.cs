using Jumia_Clone.Data;
using Jumia_Clone.Repositories;
using Jumia_Clone.Repositories.Implementation;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services;
using Jumia_Clone.Services.Implementation;
using Jumia_Clone.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Threading.RateLimiting;
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
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // ✅ Add in-memory cache
            services.AddMemoryCache();

            // ✅ Add Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("standard", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("strict", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

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
            app.UseStaticFiles(); 

            // Use CORS
            app.UseCors("CorsPolicy");


            // ✅ Enable rate limiter middleware
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // JWT Service
            services.AddScoped<JwtService>();

            // Subcategory Service
            //services.AddScoped<SubcategoryService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Images Service
            services.AddScoped<IImageService, ImageService>();

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
            // Subcategory repository
            services.AddScoped<ISubcategoryService, SubcategoryRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            // ✅ Add this line for Order Repository
            services.AddScoped<IOrderRepository, OrderRepository>();
            // ✅ Address repository
            services.AddScoped<IAddressRepository, AddressRepository>();
            // category repository
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            // product repository
            services.AddScoped<IProductRepository, ProductRepository>();
            // Add other repositories here as your project grows
            // Example: services.AddScoped<IProductRepository, ProductRepository>();
            // Example: services.AddScoped<IOrderRepository, OrderRepository>();
        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("sqlCon")));
        }
    }
}