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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Skapa en test-fastighet
            modelBuilder.Entity<Property>().HasData(
                new Property { PropertyId = 1, Address = "Storgatan 1", City = "Stockholm", PostalCode = "111 22" }
            );

            // 2. Skapa en test-hyresgäst
            modelBuilder.Entity<Tenant>().HasData(
                new Tenant { TenantID = 1, Name = "Viktor Nilsson", Phone = "070-1234567", Email = "viktor@test.com" }
            );

            // 3. Skapa en lägenhet som kopplar ihop båda
            modelBuilder.Entity<Apartment>().HasData(
                new Apartment { ApartmentID = 1, PropertyID = 1, TenantID = 1, ApartmentNumber = "999", Size = 50, Rent = 5000 }
            );
        }
    }


}