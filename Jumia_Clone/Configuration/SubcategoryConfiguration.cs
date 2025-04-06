using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Repositories.Implementation;
using Jumia_Clone.Services;

namespace Jumia_Clone.Configuration
{
    public static class SubcategoryConfiguration
    {
        
             public static IServiceCollection AddSubcategoryServices(this IServiceCollection services)
             {
                services.AddScoped<ISubcategoryService, SubcategoryRepository>(); 
                services.AddScoped<SubcategoryService>();

                return services;
       
             }
    }
}
