using AdvertisingPlatforms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace AdvertisingPlatforms
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers(); // For API controllers
            builder.Services.AddRazorPages(); // For Razor Pages
            builder.Services.AddSingleton<Tree>(); // In-memory Tree

            var app = builder.Build();

            //app.UseHttpsRedirection(); // TODO: добавить сертификат для https в продакшене.
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers(); // Map API controllers
            app.MapRazorPages(); // Map Razor Pages

            app.Run("http://localhost:5000");
        }
    }
}