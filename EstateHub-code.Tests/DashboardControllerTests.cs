using System.Text.Json;
using EstateHub_code.Controllers;
using EstateHub_code.Data;
using EstateHub_code.Models;
using EstateHub_code.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstateHub_code.Tests
{
    public class DashboardControllerTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOk()
        {
            using var context = CreateContext();
            context.AdminProfiles.Add(new AdminProfile
            {
                Email = "test@estatehub.com",
                PasswordHash = PasswordHasher.Hash("secret123")
            });
            await context.SaveChangesAsync();
            var controller = new DashboardController(context);

            var result = await controller.Login(new DashboardController.LoginRequest
            {
                Email = "test@estatehub.com",
                Password = "secret123"
            });

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            using var context = CreateContext();
            context.AdminProfiles.Add(new AdminProfile
            {
                Email = "test@estatehub.com",
                PasswordHash = PasswordHasher.Hash("secret123")
            });
            await context.SaveChangesAsync();
            var controller = new DashboardController(context);

            var result = await controller.Login(new DashboardController.LoginRequest
            {
                Email = "test@estatehub.com",
                Password = "wrong-password"
            });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
        {
            using var context = CreateContext();
            context.AdminProfiles.Add(new AdminProfile
            {
                Email = "test@estatehub.com",
                PasswordHash = PasswordHasher.Hash("secret123")
            });
            await context.SaveChangesAsync();
            var controller = new DashboardController(context);

            var result = await controller.Login(new DashboardController.LoginRequest
            {
                Email = "nobody@estatehub.com",
                Password = "secret123"
            });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetInspectionReport_ForInspectionWithoutSavedReport_ReturnsScaffoldWithFormattedAddress()
        {
            using var context = CreateContext();
            // PropertyId 1 is seeded by AppDbContext.OnModelCreating (Storgatan 1, Stockholm, 111 22).
            context.Apartments.Add(new Apartment
            {
                ApartmentID = 2,
                PropertyID = 1,
                ApartmentNumber = "101",
                Size = 40,
                Rent = 4000,
                Rooms = 1,
                Floor = 2,
                Status = "Uthyrd"
            });
            context.Inspections.Add(new Inspection
            {
                InspectionId = 10,
                ApartmentNumber = "101",
                InspectionDate = new DateTime(2026, 1, 15),
                Inspector = "Anna Svensson",
                Status = "Planned"
            });
            await context.SaveChangesAsync();
            var controller = new DashboardController(context);

            var result = await controller.GetInspectionReport(10);

            var json = JsonSerializer.Serialize(result.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.GetProperty("isNew").GetBoolean());
            Assert.Equal("Storgatan 1, Apartment 101, 111 22 Stockholm", root.GetProperty("propertyAddress").GetString());
            Assert.Equal("Anna Svensson", root.GetProperty("inspectorName").GetString());
            Assert.True(root.GetProperty("roomItems").GetArrayLength() > 0);
            Assert.True(root.GetProperty("utilities").GetArrayLength() > 0);
        }

        [Fact]
        public async Task GetInspectionReport_ForUnknownInspection_ReturnsNotFound()
        {
            using var context = CreateContext();
            var controller = new DashboardController(context);

            var result = await controller.GetInspectionReport(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
