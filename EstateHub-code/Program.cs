using EstateHub_code.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EstateHub_code
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Hamta connection string fran appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // 2. Registrera tjanster (DbContext maste ligga har, fore builder.Build)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            builder.Services.AddControllers()
             .AddJsonOptions(options =>
             {
                 // Detta forhindrar att JSON-datan snurrar runt i cirklar
                 options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
             });
            builder.Services.AddOpenApi();

            // 3. Nu bygger vi appen - efter detta ar ritningen last
            var app = builder.Build();

            EnsureDatabaseColumns(app);
            SeedDemoData(app);

            // 4. Konfigurera hur appen ska bete sig (Middleware)
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = { "dashboard.html" }
            });
            app.UseStaticFiles();  // Tillater att servern skickar HTML/CSS/JS-filer
            app.UseAuthorization();
            app.MapControllers();

            // 5. Starta!
            app.Run();
        }

        private static void EnsureDatabaseColumns(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var apartmentColumns = new Dictionary<string, string>
            {
                ["Rooms"] = "int NOT NULL DEFAULT 0",
                ["Floor"] = "int NOT NULL DEFAULT 0",
                ["Status"] = "varchar(50) NOT NULL DEFAULT 'Ledig'",
                ["AvailableFrom"] = "datetime NULL",
                ["ElectricityIncluded"] = "tinyint(1) NOT NULL DEFAULT 0",
                ["WaterIncluded"] = "tinyint(1) NOT NULL DEFAULT 0",
                ["InternetIncluded"] = "tinyint(1) NOT NULL DEFAULT 0",
                ["Balcony"] = "tinyint(1) NOT NULL DEFAULT 0",
                ["Furnished"] = "tinyint(1) NOT NULL DEFAULT 0"
            };

            EnsureColumns(connection, "apartments", apartmentColumns);

            var tenantColumns = new Dictionary<string, string>
            {
                ["PersonalNumber"] = "varchar(20) NULL",
                ["Address"] = "varchar(200) NULL",
                ["MobilePhone"] = "varchar(30) NULL"
            };

            EnsureColumns(connection, "tenants", tenantColumns);
        }

        private static void EnsureColumns(System.Data.Common.DbConnection connection, string tableName, Dictionary<string, string> columns)
        {
            foreach (var column in columns)
            {
                if (ColumnExists(connection, tableName, column.Key))
                {
                    continue;
                }

                using var command = connection.CreateCommand();
                command.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN `{column.Key}` {column.Value};";
                command.ExecuteNonQuery();
            }
        }

        private static void SeedDemoData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!context.Tenants.Any(t => t.PersonalNumber == "19900101-1234"))
            {
                context.Tenants.Add(new EstateHub_code.Models.Tenant
                {
                    FirstName = "Viktor",
                    LastName = "Nilsson",
                    PersonalNumber = "19900101-1234",
                    Address = "Storgatan 1, 111 22 Stockholm",
                    MobilePhone = "070-123 45 67",
                    Phone = "08-123 456",
                    Email = "viktor@test.com"
                });
            }

            if (!context.Tenants.Any(t => t.PersonalNumber == "19880512-5678"))
            {
                context.Tenants.Add(new EstateHub_code.Models.Tenant
                {
                    FirstName = "Elsa",
                    LastName = "Andersson",
                    PersonalNumber = "19880512-5678",
                    Address = "Parkvägen 8, 112 40 Stockholm",
                    MobilePhone = "073-222 33 44",
                    Phone = "",
                    Email = "elsa@test.com"
                });
            }

            if (!context.Tenants.Any(t => t.PersonalNumber == "19791130-9012"))
            {
                context.Tenants.Add(new EstateHub_code.Models.Tenant
                {
                    FirstName = "Amir",
                    LastName = "Hassan",
                    PersonalNumber = "19791130-9012",
                    Address = "Solgatan 12, 113 50 Stockholm",
                    MobilePhone = "076-555 66 77",
                    Phone = "",
                    Email = "amir@test.com"
                });
            }

            context.SaveChanges();

            var viktor = context.Tenants.FirstOrDefault(t => t.PersonalNumber == "19900101-1234");
            var elsa = context.Tenants.FirstOrDefault(t => t.PersonalNumber == "19880512-5678");

            if (!context.Apartments.Any(a => a.ApartmentNumber == "1001"))
            {
                context.Apartments.Add(new EstateHub_code.Models.Apartment
                {
                    PropertyID = 1,
                    ApartmentNumber = "1001",
                    Size = 38,
                    Rent = 7200,
                    Rooms = 1,
                    Floor = 1,
                    Status = "Ledig",
                    AvailableFrom = DateTime.Today.AddDays(14),
                    ElectricityIncluded = true,
                    WaterIncluded = true,
                    InternetIncluded = false,
                    Balcony = false,
                    Furnished = false
                });
            }

            if (!context.Apartments.Any(a => a.ApartmentNumber == "1203"))
            {
                context.Apartments.Add(new EstateHub_code.Models.Apartment
                {
                    PropertyID = 1,
                    TenantID = elsa?.TenantID,
                    ApartmentNumber = "1203",
                    Size = 62,
                    Rent = 10500,
                    Rooms = 2,
                    Floor = 2,
                    Status = "Reserverad",
                    AvailableFrom = DateTime.Today.AddMonths(1),
                    ElectricityIncluded = false,
                    WaterIncluded = true,
                    InternetIncluded = true,
                    Balcony = true,
                    Furnished = false
                });
            }

            if (!context.Apartments.Any(a => a.ApartmentNumber == "1402"))
            {
                context.Apartments.Add(new EstateHub_code.Models.Apartment
                {
                    PropertyID = 1,
                    TenantID = viktor?.TenantID,
                    ApartmentNumber = "1402",
                    Size = 84,
                    Rent = 14200,
                    Rooms = 3,
                    Floor = 4,
                    Status = "Renoveras",
                    AvailableFrom = DateTime.Today.AddMonths(2),
                    ElectricityIncluded = false,
                    WaterIncluded = true,
                    InternetIncluded = false,
                    Balcony = true,
                    Furnished = true
                });
            }

            context.SaveChanges();
        }

        private static bool ColumnExists(System.Data.Common.DbConnection connection, string tableName, string columnName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*)
                FROM information_schema.columns
                WHERE table_schema = DATABASE()
                    AND table_name = @tableName
                    AND column_name = @columnName;";

            var tableParameter = command.CreateParameter();
            tableParameter.ParameterName = "@tableName";
            tableParameter.Value = tableName;
            command.Parameters.Add(tableParameter);

            var columnParameter = command.CreateParameter();
            columnParameter.ParameterName = "@columnName";
            columnParameter.Value = columnName;
            command.Parameters.Add(columnParameter);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}
