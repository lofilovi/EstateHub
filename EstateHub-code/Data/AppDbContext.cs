using Microsoft.EntityFrameworkCore;
using EstateHub_code.Models;

namespace EstateHub_code.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<CustomerIssue> CustomerIssues { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<AdminProfile> AdminProfiles { get; set; }
        public DbSet<AccountingRecord> AccountingRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Skapa en test-fastighet
            modelBuilder.Entity<Property>().HasData(
                new Property { PropertyId = 1, Address = "Storgatan 1", City = "Stockholm", PostalCode = "111 22" }
            );

            // 2. Skapa en test-hyresgäst
            // 2. Skapa en test-hyresgäst (Uppdaterad med för- och efternamn)
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant
                {
                    TenantID = 1,
                    FirstName = "Viktor",
                    LastName = "Nilsson",
                    Phone = "070-1234567",
                    Email = "viktor@test.com"
                }
            );

            // 3. Skapa en lägenhet som kopplar ihop båda
            modelBuilder.Entity<Apartment>().HasData(
                new Apartment
                {
                    ApartmentID = 1,
                    PropertyID = 1,
                    TenantID = 1,
                    ApartmentNumber = "999",
                    Size = 50,
                    Rent = 5000,
                    Rooms = 2,
                    Floor = 3,
                    Status = "Uthyrd",
                    ElectricityIncluded = false,
                    WaterIncluded = true,
                    InternetIncluded = false,
                    Balcony = true,
                    Furnished = false,
                    ImageUrl = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?q=80&w=1200"
                }
            );
        }
    }


}
