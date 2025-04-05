
using Jumia_Clone.Configuration;
using Jumia_Clone.Data;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            // Configure CORS
            builder.Services.ConfigureCors(builder.Configuration);
            // Add services to the container
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("sqlCon")));
        

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Use CORS
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
