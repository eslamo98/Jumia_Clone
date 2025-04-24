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
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using Jumia_Clone.MappingProfiles;
using Jumia_Clone.CustomException;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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

            // Add in-memory cache
            //services.AddMemoryCache();

            //Add global exception handler
            //services.AddExceptionHandler<GlobalExceptionHandler>();
            //services.AddProblemDetails();
            // Add Rate Limiting
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

        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer(); // Add this line - it's important!

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jumia Clone API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static WebApplication ConfigureMiddleware(this WebApplication app)
        {
            // Add this middleware after authentication middleware
            app.Use(async (context, next) =>
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                    {
                        var userRepository = context.RequestServices.GetRequiredService<IUserRepository>();
                        var user = await userRepository.GetUserByIdAsync(userId);

                        if (user == null || user.IsActive != true)
                        {
                            // User is inactive or deleted, log them out
                            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                            // If it's an API request, return 401 Unauthorized
                            if (context.Request.Path.StartsWithSegments("/api"))
                            {
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";

                                var response = new ApiErrorResponse
                                {
                                    Message = "Your account has been deactivated",
                                    ErrorMessages = new string[] { "Please contact support for more information" }
                                };

                                await context.Response.WriteAsJsonAsync(response);
                                return;
                            }
                            else
                            {
                                // For non-API requests, redirect to login page
                                context.Response.Redirect("/login");
                                return;
                            }
                        }
                    }
                }

                await next();
            });
            // Configure detailed exception handling
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error != null)
                    {
                        var exception = exceptionHandlerPathFeature.Error;

                        // You can log the exception here if needed
                        // _logger.LogError(exception, "An unhandled exception occurred.");

                        var errorResponse = new ErrorResponse(
                            "An unexpected error occurred",
                            "UNEXPECTED_ERROR",
                            new List<string> { exception.Message }
                        );

                        await context.Response.WriteAsJsonAsync(errorResponse);
                    }
                });
            });
            // Always enable Swagger for all environments
            app.UseSwagger(c =>
            {
                // This fixes the issue with the 404 error
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Jumia Clone API v1");
                options.DocExpansion(DocExpansion.None);
                options.DefaultModelsExpandDepth(-1);
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Use CORS
            app.UseCors("CorsPolicy");

            // Use Rate Limiter
            //app.UseRateLimiter();

            // This order is important!
            app.UseAuthentication(); // Must come before UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
        private static void RegisterServices(IServiceCollection services)
        {
            // JWT Service
            services.AddScoped<JwtService>();

            // Other services
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Images Service
            services.AddScoped<IImageService, ImageService>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<CouponMappingProfile>();
                cfg.AddProfile<WishlistMappingProfile>();
            });
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // User repository
            services.AddScoped<IUserRepository, UserRepository>();

            // Auth repository
            services.AddScoped<IAuthRepository, AuthRepository>();

            // Subcategory repository
            services.AddScoped<ISubcategoryRepository, SubcategoriesRepository>();
            services.AddScoped<ICartRepository, CartRepository>();

            // Order Repository
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Address repository
            services.AddScoped<IAddressRepository, AddressRepository>();

            // Category repository
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            // Product repository
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IGetAllRepository, GetAllRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IProductVariantsRepository, ProductVariantsRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();
            
            

        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("sqlCon")));
        }
    }
}