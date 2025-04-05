using Jumia_Clone.Configuration;

namespace Jumia_Clone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure all services through our centralized configuration
            builder.Services.ConfigureServices(builder.Configuration);

            var app = builder.Build();

            // Configure all middleware through our centralized configuration
            app.ConfigureMiddleware();

            app.Run();
        }
    }
}