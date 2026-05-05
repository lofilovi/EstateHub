using EstateHub_code.Data;
using Microsoft.EntityFrameworkCore;

namespace EstateHub_code
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Hämta connection string från appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // 2. Registrera tjänster (DbContext måste ligga HÄR, före builder.Build)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            builder.Services.AddControllers()
             .AddJsonOptions(options =>
             {
                 // Detta förhindrar att JSON-datan "snurrar runt" i cirklar
                 options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
             });
            builder.Services.AddOpenApi();

            // 3. Nu bygger vi appen - efter detta är "ritningen" låst
            var app = builder.Build();

            // 4. Konfigurera hur appen ska bete sig (Middleware)
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles(); // Gör att den letar efter index.html automatiskt
            app.UseStaticFiles();  // Tillåter att servern skickar HTML/CSS/JS-filer
            app.UseAuthorization();
            app.MapControllers();

            // 5. Starta!
            app.Run();
        }
    }
}