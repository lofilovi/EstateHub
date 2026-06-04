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
            EnsureDashboardTables(app);
            SeedDemoData(app);
            SeedDashboardData(app);

            // 4. Konfigurera hur appen ska bete sig (Middleware)
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

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
                ["Furnished"] = "tinyint(1) NOT NULL DEFAULT 0",
                ["ImageUrl"] = "varchar(500) NOT NULL DEFAULT ''"
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

            void AddPropertyIfMissing(string address, string city, string postalCode)
            {
                if (!context.Properties.Any(property => property.Address == address))
                {
                    context.Properties.Add(new EstateHub_code.Models.Property
                    {
                        Address = address,
                        City = city,
                        PostalCode = postalCode
                    });
                }
            }

            AddPropertyIfMissing("Storgatan 1", "Stockholm", "111 22");
            AddPropertyIfMissing("Parkvägen 8", "Stockholm", "112 40");
            AddPropertyIfMissing("Solgatan 12", "Stockholm", "113 50");
            AddPropertyIfMissing("Kungsgatan 24", "Stockholm", "111 35");
            AddPropertyIfMissing("Linnégatan 6", "Göteborg", "413 04");
            AddPropertyIfMissing("Södra Förstadsgatan 18", "Malmö", "211 43");
            context.SaveChanges();

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

            if (!context.Tenants.Any(t => t.PersonalNumber == "19950318-2468"))
            {
                context.Tenants.Add(new EstateHub_code.Models.Tenant
                {
                    FirstName = "Maja",
                    LastName = "Berg",
                    PersonalNumber = "19950318-2468",
                    Address = "Kungsgatan 24, 111 35 Stockholm",
                    MobilePhone = "072-444 55 66",
                    Phone = "",
                    Email = "maja@test.com"
                });
            }

            if (!context.Tenants.Any(t => t.PersonalNumber == "19821205-1357"))
            {
                context.Tenants.Add(new EstateHub_code.Models.Tenant
                {
                    FirstName = "Noah",
                    LastName = "Lind",
                    PersonalNumber = "19821205-1357",
                    Address = "Linnégatan 6, 413 04 Göteborg",
                    MobilePhone = "079-888 77 66",
                    Phone = "",
                    Email = "noah@test.com"
                });
            }

            context.SaveChanges();

            var viktor = context.Tenants.FirstOrDefault(t => t.PersonalNumber == "19900101-1234");
            var elsa = context.Tenants.FirstOrDefault(t => t.PersonalNumber == "19880512-5678");
            var maja = context.Tenants.FirstOrDefault(t => t.PersonalNumber == "19950318-2468");
            var noah = context.Tenants.FirstOrDefault(t => t.PersonalNumber == "19821205-1357");

            var properties = context.Properties.ToDictionary(property => property.Address, property => property.PropertyId);
            var apartmentImages = new Dictionary<string, string>
            {
                ["999"] = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?q=80&w=1200",
                ["1001"] = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?q=80&w=1200",
                ["1203"] = "https://images.unsplash.com/photo-1494526585095-c41746248156?q=80&w=1200",
                ["1402"] = "https://images.unsplash.com/photo-1484154218962-a197022b5858?q=80&w=1200",
                ["0802"] = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?q=80&w=1200",
                ["2104"] = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?q=80&w=1200",
                ["0301"] = "https://images.unsplash.com/photo-1512918728675-ed5a9ecdebfd?q=80&w=1200",
                ["1505"] = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?q=80&w=1200",
                ["0707"] = "https://images.unsplash.com/photo-1554995207-c18c203602cb?q=80&w=1200",
                ["1102"] = "https://images.unsplash.com/photo-1616486338812-3dadae4b4ace?q=80&w=1200",
                ["1901"] = "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?q=80&w=1200",
                ["0404"] = "https://images.unsplash.com/photo-1600566753086-00f18fb6b3ea?q=80&w=1200",
                ["1608"] = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?q=80&w=1200"
            };

            void AddApartmentIfMissing(
                string number,
                string address,
                double size,
                decimal rent,
                int rooms,
                int floor,
                string status,
                DateTime? availableFrom,
                bool electricityIncluded,
                bool waterIncluded,
                bool internetIncluded,
                bool balcony,
                bool furnished,
                int? tenantId = null)
            {
                var imageUrl = apartmentImages[number];
                var existing = context.Apartments.FirstOrDefault(apartment => apartment.ApartmentNumber == number);
                if (existing != null)
                {
                    existing.ImageUrl = string.IsNullOrWhiteSpace(existing.ImageUrl) ? imageUrl : existing.ImageUrl;
                    return;
                }

                context.Apartments.Add(new EstateHub_code.Models.Apartment
                {
                    PropertyID = properties[address],
                    TenantID = tenantId,
                    ApartmentNumber = number,
                    Size = size,
                    Rent = rent,
                    Rooms = rooms,
                    Floor = floor,
                    Status = status,
                    AvailableFrom = availableFrom,
                    ElectricityIncluded = electricityIncluded,
                    WaterIncluded = waterIncluded,
                    InternetIncluded = internetIncluded,
                    Balcony = balcony,
                    Furnished = furnished,
                    ImageUrl = imageUrl
                });
            }

            AddApartmentIfMissing("1001", "Storgatan 1", 38, 7200, 1, 1, "Ledig", DateTime.Today.AddDays(14), true, true, false, false, false);
            AddApartmentIfMissing("1203", "Storgatan 1", 62, 10500, 2, 2, "Reserverad", DateTime.Today.AddMonths(1), false, true, true, true, false, elsa?.TenantID);
            AddApartmentIfMissing("1402", "Storgatan 1", 84, 14200, 3, 4, "Renoveras", DateTime.Today.AddMonths(2), false, true, false, true, true, viktor?.TenantID);
            AddApartmentIfMissing("0802", "Parkvägen 8", 45, 8900, 2, 2, "Ledig", DateTime.Today.AddDays(7), true, true, true, true, false);
            AddApartmentIfMissing("2104", "Parkvägen 8", 72, 12800, 3, 5, "Uthyrd", null, false, true, true, true, true, maja?.TenantID);
            AddApartmentIfMissing("0301", "Solgatan 12", 29, 6100, 1, 0, "Ledig", DateTime.Today.AddDays(3), true, true, false, false, true);
            AddApartmentIfMissing("1505", "Solgatan 12", 96, 16800, 4, 6, "Uthyrd", null, false, true, true, true, false, noah?.TenantID);
            AddApartmentIfMissing("0707", "Kungsgatan 24", 54, 11200, 2, 3, "Ledig", DateTime.Today.AddDays(21), false, true, true, false, false);
            AddApartmentIfMissing("1102", "Kungsgatan 24", 68, 13400, 3, 4, "Reserverad", DateTime.Today.AddMonths(1).AddDays(10), true, true, true, true, true);
            AddApartmentIfMissing("1901", "Linnégatan 6", 41, 8300, 1, 1, "Ledig", DateTime.Today.AddDays(30), true, true, false, true, false);
            AddApartmentIfMissing("0404", "Linnégatan 6", 77, 13900, 3, 2, "Renoveras", DateTime.Today.AddMonths(3), false, true, true, false, true);
            AddApartmentIfMissing("1608", "Södra Förstadsgatan 18", 88, 15100, 4, 5, "Ledig", DateTime.Today.AddDays(45), false, true, true, true, false);

            foreach (var apartment in context.Apartments.Where(apartment => string.IsNullOrWhiteSpace(apartment.ImageUrl)))
            {
                apartment.ImageUrl = apartmentImages.TryGetValue(apartment.ApartmentNumber, out var imageUrl)
                    ? imageUrl
                    : apartmentImages["1001"];
            }

            context.SaveChanges();
        }

        private static void EnsureDashboardTables(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var statements = new[]
            {
                """
                CREATE TABLE IF NOT EXISTS calendar_events (
                    CalendarEventId int NOT NULL AUTO_INCREMENT,
                    Title varchar(200) NOT NULL,
                    Category varchar(100) NOT NULL,
                    Location varchar(200) NOT NULL,
                    StartsAt datetime NOT NULL,
                    PRIMARY KEY (CalendarEventId)
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS customer_issues (
                    CustomerIssueId int NOT NULL AUTO_INCREMENT,
                    CustomerName varchar(200) NOT NULL,
                    Issue varchar(200) NOT NULL,
                    Status varchar(50) NOT NULL,
                    Priority varchar(50) NOT NULL,
                    PRIMARY KEY (CustomerIssueId)
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS work_orders (
                    WorkOrderId int NOT NULL AUTO_INCREMENT,
                    OrderNumber varchar(50) NOT NULL,
                    Supplier varchar(100) NOT NULL,
                    Product varchar(200) NOT NULL,
                    Status varchar(50) NOT NULL,
                    OrderDate datetime NOT NULL,
                    PRIMARY KEY (WorkOrderId)
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS inspections (
                    InspectionId int NOT NULL AUTO_INCREMENT,
                    ApartmentNumber varchar(50) NOT NULL,
                    InspectionDate datetime NOT NULL,
                    Inspector varchar(200) NOT NULL,
                    Status varchar(50) NOT NULL,
                    Notes varchar(500) NOT NULL,
                    PRIMARY KEY (InspectionId)
                );
                """,
                """
                CREATE TABLE IF NOT EXISTS app_settings (
                    AppSettingId int NOT NULL AUTO_INCREMENT,
                    DarkMode tinyint(1) NOT NULL DEFAULT 0,
                    EmailNotifications tinyint(1) NOT NULL DEFAULT 1,
                    AutoSaveReports tinyint(1) NOT NULL DEFAULT 1,
                    PRIMARY KEY (AppSettingId)
                );
                """
            };

            foreach (var statement in statements)
            {
                using var command = connection.CreateCommand();
                command.CommandText = statement;
                command.ExecuteNonQuery();
            }
        }

        private static void SeedDashboardData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!context.CalendarEvents.Any())
            {
                context.CalendarEvents.AddRange(
                    new EstateHub_code.Models.CalendarEvent
                    {
                        Title = "Apartment Viewing",
                        Category = "Viewing",
                        Location = "Stockholm",
                        StartsAt = DateTime.Today.AddHours(9)
                    },
                    new EstateHub_code.Models.CalendarEvent
                    {
                        Title = "Customer Contract Meeting",
                        Category = "Contract",
                        Location = "EstateHub Office",
                        StartsAt = DateTime.Today.AddHours(12).AddMinutes(30)
                    },
                    new EstateHub_code.Models.CalendarEvent
                    {
                        Title = "Inspection",
                        Category = "Inspection",
                        Location = "Storgatan 1",
                        StartsAt = DateTime.Today.AddHours(15)
                    },
                    new EstateHub_code.Models.CalendarEvent
                    {
                        Title = "Rent Payment Deadline",
                        Category = "Accounting",
                        Location = "All properties",
                        StartsAt = DateTime.Today.AddDays(7)
                    }
                );
            }

            if (!context.CustomerIssues.Any())
            {
                context.CustomerIssues.AddRange(
                    new EstateHub_code.Models.CustomerIssue { CustomerName = "Viktor Nilsson", Issue = "Heating Problem", Status = "Open", Priority = "High" },
                    new EstateHub_code.Models.CustomerIssue { CustomerName = "Elsa Andersson", Issue = "Broken Window", Status = "In Progress", Priority = "Medium" },
                    new EstateHub_code.Models.CustomerIssue { CustomerName = "Amir Hassan", Issue = "Water Leakage", Status = "Closed", Priority = "High" }
                );
            }

            if (!context.WorkOrders.Any())
            {
                context.WorkOrders.AddRange(
                    new EstateHub_code.Models.WorkOrder { OrderNumber = "#1024", Supplier = "IKEA", Product = "Kitchen Furniture", Status = "Shipping", OrderDate = DateTime.Today.AddDays(16) },
                    new EstateHub_code.Models.WorkOrder { OrderNumber = "#1025", Supplier = "JYSK", Product = "Bed Package", Status = "Pending", OrderDate = DateTime.Today.AddDays(17) },
                    new EstateHub_code.Models.WorkOrder { OrderNumber = "#1026", Supplier = "Electrolux", Product = "Washing Machine", Status = "Delivered", OrderDate = DateTime.Today.AddDays(18) }
                );
            }

            if (!context.Inspections.Any())
            {
                context.Inspections.AddRange(
                    new EstateHub_code.Models.Inspection { ApartmentNumber = "1001", InspectionDate = DateTime.Today.AddDays(16), Inspector = "Adam Smith", Status = "Planned", Notes = "Standard move-in check" },
                    new EstateHub_code.Models.Inspection { ApartmentNumber = "1203", InspectionDate = DateTime.Today.AddDays(19), Inspector = "Emma Nilsson", Status = "Planned", Notes = "Balcony and bathroom" },
                    new EstateHub_code.Models.Inspection { ApartmentNumber = "1402", InspectionDate = DateTime.Today.AddDays(22), Inspector = "Johan Berg", Status = "Planned", Notes = "Renovation follow-up" }
                );
            }

            if (!context.AppSettings.Any())
            {
                context.AppSettings.Add(new EstateHub_code.Models.AppSetting());
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
