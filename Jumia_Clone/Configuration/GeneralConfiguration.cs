using Jumia_Clone.Data;
using Jumia_Clone.Repositories;
using Jumia_Clone.Repositories.Implementation;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services;
using Jumia_Clone.Services.Implementation;
using Jumia_Clone.Services.Interfaces;
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
            app.UseStaticFiles(); 

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

            //Images Service
            services.AddScoped<IImageService, ImageService>();
        
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

            // category repository
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            // product repository
            services.AddScoped<IProductRepository, ProductRepository>();
        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("sqlCon")));
        }
    }
}