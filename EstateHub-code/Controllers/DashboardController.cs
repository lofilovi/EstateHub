using EstateHub_code.Data;
using EstateHub_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EstateHub_code.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("calendar")]
        public async Task<ActionResult<IEnumerable<CalendarEvent>>> GetCalendarEvents()
        {
            return await _context.CalendarEvents
                .OrderBy(calendarEvent => calendarEvent.StartsAt)
                .ToListAsync();
        }

        [HttpPost("calendar")]
        public async Task<ActionResult<CalendarEvent>> PostCalendarEvent(CalendarEvent calendarEvent)
        {
            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();
            return Ok(calendarEvent);
        }

        [HttpGet("customer-issues")]
        public async Task<ActionResult<IEnumerable<CustomerIssue>>> GetCustomerIssues()
        {
            return await _context.CustomerIssues
                .OrderBy(issue => issue.Status)
                .ThenBy(issue => issue.Priority)
                .ToListAsync();
        }

        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> GetOrders()
        {
            return await _context.WorkOrders
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();
        }

        [HttpGet("inspections")]
        public async Task<ActionResult<IEnumerable<Inspection>>> GetInspections()
        {
            return await _context.Inspections
                .OrderBy(inspection => inspection.InspectionDate)
                .ToListAsync();
        }

        public class RoomItemDto
        {
            public string Room { get; set; } = "";
            public string Item { get; set; } = "";
            public string Condition { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        public class UtilityItemDto
        {
            public string System { get; set; } = "";
            public string Condition { get; set; } = "";
            public string Notes { get; set; } = "";
        }

        public class MeterReadingDto
        {
            public string Meter { get; set; } = "";
            public string Reading { get; set; } = "";
            public string ReadingDate { get; set; } = "";
        }

        public class IssueDto
        {
            public string Issue { get; set; } = "";
            public string Location { get; set; } = "";
            public string Priority { get; set; } = "Medium";
            public string PhotoRef { get; set; } = "";
        }

        public class PhotoDto
        {
            public string Label { get; set; } = "";
            public string Description { get; set; } = "";
        }

        public class InspectionReportRequest
        {
            public string PropertyAddress { get; set; } = "";
            public string InspectionType { get; set; } = "Move-in";
            public DateTime InspectionDate { get; set; }
            public string InspectorName { get; set; } = "";
            public string PresentDuringInspection { get; set; } = "";
            public string WeatherConditions { get; set; } = "";
            public string OverallCondition { get; set; } = "Good";
            public string Summary { get; set; } = "";
            public List<RoomItemDto> RoomItems { get; set; } = new();
            public List<UtilityItemDto> Utilities { get; set; } = new();
            public List<MeterReadingDto> MeterReadings { get; set; } = new();
            public List<string> IncludedItems { get; set; } = new();
            public List<IssueDto> Issues { get; set; } = new();
            public List<PhotoDto> Photos { get; set; } = new();
            public string TenantSignatureName { get; set; } = "";
            public string TenantSignatureDate { get; set; } = "";
            public string LandlordSignatureName { get; set; } = "";
            public string LandlordSignatureDate { get; set; } = "";
        }

        private static List<RoomItemDto> DefaultRoomItems() => new()
        {
            new() { Room = "Entrance / Hallway", Item = "Floor", Condition = "Good" },
            new() { Room = "Entrance / Hallway", Item = "Walls", Condition = "Good" },
            new() { Room = "Entrance / Hallway", Item = "Door lock", Condition = "Good" },
            new() { Room = "Entrance / Hallway", Item = "Lighting", Condition = "Good" },
            new() { Room = "Living Room", Item = "Walls/ceiling", Condition = "Good" },
            new() { Room = "Living Room", Item = "Floor", Condition = "Good" },
            new() { Room = "Living Room", Item = "Windows", Condition = "Good" },
            new() { Room = "Living Room", Item = "Radiator", Condition = "Good" },
            new() { Room = "Living Room", Item = "Electrical outlets", Condition = "Good" },
            new() { Room = "Kitchen", Item = "Faucet", Condition = "Good" },
            new() { Room = "Kitchen", Item = "Cabinets", Condition = "Good" },
            new() { Room = "Kitchen", Item = "Stove/oven", Condition = "Good" },
            new() { Room = "Kitchen", Item = "Refrigerator", Condition = "Good" },
            new() { Room = "Kitchen", Item = "Ventilation/exhaust fan", Condition = "Good" },
            new() { Room = "Kitchen", Item = "Countertop", Condition = "Good" },
            new() { Room = "Bedroom", Item = "Walls/ceiling", Condition = "Good" },
            new() { Room = "Bedroom", Item = "Floor", Condition = "Good" },
            new() { Room = "Bedroom", Item = "Window", Condition = "Good" },
            new() { Room = "Bedroom", Item = "Closet", Condition = "Good" },
            new() { Room = "Bathroom", Item = "Toilet", Condition = "Good" },
            new() { Room = "Bathroom", Item = "Sink", Condition = "Good" },
            new() { Room = "Bathroom", Item = "Shower/bath", Condition = "Good" },
            new() { Room = "Bathroom", Item = "Ventilation fan", Condition = "Good" },
            new() { Room = "Bathroom", Item = "Mold/mildew check", Condition = "None found" }
        };

        private static List<UtilityItemDto> DefaultUtilities() => new()
        {
            new() { System = "Electrical panel", Condition = "Good" },
            new() { System = "Water heater", Condition = "Good" },
            new() { System = "Heating system", Condition = "Good" },
            new() { System = "Smoke detectors", Condition = "Good" }
        };

        private static List<MeterReadingDto> DefaultMeterReadings() => new()
        {
            new() { Meter = "Electricity" },
            new() { Meter = "Water" }
        };

        private async Task<object> BuildInspectionReportDto(int inspectionId)
        {
            var report = await _context.InspectionReports.FirstOrDefaultAsync(r => r.InspectionId == inspectionId);

            if (report != null)
            {
                return new
                {
                    report.InspectionReportId,
                    report.InspectionId,
                    report.PropertyAddress,
                    report.InspectionType,
                    report.InspectionDate,
                    report.InspectorName,
                    report.PresentDuringInspection,
                    report.WeatherConditions,
                    report.OverallCondition,
                    report.Summary,
                    roomItems = JsonSerializer.Deserialize<List<RoomItemDto>>(report.RoomItemsJson) ?? new(),
                    utilities = JsonSerializer.Deserialize<List<UtilityItemDto>>(report.UtilitiesJson) ?? new(),
                    meterReadings = JsonSerializer.Deserialize<List<MeterReadingDto>>(report.MeterReadingsJson) ?? new(),
                    includedItems = JsonSerializer.Deserialize<List<string>>(report.IncludedItemsJson) ?? new(),
                    issues = JsonSerializer.Deserialize<List<IssueDto>>(report.IssuesJson) ?? new(),
                    photos = JsonSerializer.Deserialize<List<PhotoDto>>(report.PhotosJson) ?? new(),
                    report.TenantSignatureName,
                    report.TenantSignatureDate,
                    report.LandlordSignatureName,
                    report.LandlordSignatureDate,
                    isNew = false
                };
            }

            var inspection = await _context.Inspections.FindAsync(inspectionId);
            var apartment = inspection == null
                ? null
                : await _context.Apartments.Include(a => a.Property)
                    .FirstOrDefaultAsync(a => a.ApartmentNumber == inspection.ApartmentNumber);

            var address = apartment?.Property != null
                ? $"{apartment.Property.Address}, Apartment {apartment.ApartmentNumber}, {apartment.Property.PostalCode} {apartment.Property.City}"
                : $"Apartment {inspection?.ApartmentNumber}";

            return new
            {
                inspectionReportId = 0,
                inspectionId,
                propertyAddress = address,
                inspectionType = "Move-in",
                inspectionDate = inspection?.InspectionDate ?? DateTime.Today,
                inspectorName = inspection?.Inspector ?? "",
                presentDuringInspection = "",
                weatherConditions = "",
                overallCondition = "Good",
                summary = inspection?.Notes ?? "",
                roomItems = DefaultRoomItems(),
                utilities = DefaultUtilities(),
                meterReadings = DefaultMeterReadings(),
                includedItems = new List<string>(),
                issues = new List<IssueDto>(),
                photos = new List<PhotoDto>(),
                tenantSignatureName = "",
                tenantSignatureDate = "",
                landlordSignatureName = "",
                landlordSignatureDate = "",
                isNew = true
            };
        }

        [HttpGet("inspections/{inspectionId}/report")]
        public async Task<ActionResult<object>> GetInspectionReport(int inspectionId)
        {
            var inspectionExists = await _context.Inspections.AnyAsync(i => i.InspectionId == inspectionId);
            if (!inspectionExists)
            {
                return NotFound();
            }

            return await BuildInspectionReportDto(inspectionId);
        }

        [HttpPut("inspections/{inspectionId}/report")]
        public async Task<ActionResult<object>> PutInspectionReport(int inspectionId, InspectionReportRequest request)
        {
            var report = await _context.InspectionReports.FirstOrDefaultAsync(r => r.InspectionId == inspectionId);

            if (report == null)
            {
                report = new InspectionReport { InspectionId = inspectionId };
                _context.InspectionReports.Add(report);
            }

            report.PropertyAddress = request.PropertyAddress;
            report.InspectionType = request.InspectionType;
            report.InspectionDate = request.InspectionDate;
            report.InspectorName = request.InspectorName;
            report.PresentDuringInspection = request.PresentDuringInspection;
            report.WeatherConditions = request.WeatherConditions;
            report.OverallCondition = request.OverallCondition;
            report.Summary = request.Summary;
            report.RoomItemsJson = JsonSerializer.Serialize(request.RoomItems);
            report.UtilitiesJson = JsonSerializer.Serialize(request.Utilities);
            report.MeterReadingsJson = JsonSerializer.Serialize(request.MeterReadings);
            report.IncludedItemsJson = JsonSerializer.Serialize(request.IncludedItems);
            report.IssuesJson = JsonSerializer.Serialize(request.Issues);
            report.PhotosJson = JsonSerializer.Serialize(request.Photos);
            report.TenantSignatureName = request.TenantSignatureName;
            report.TenantSignatureDate = request.TenantSignatureDate;
            report.LandlordSignatureName = request.LandlordSignatureName;
            report.LandlordSignatureDate = request.LandlordSignatureDate;

            await _context.SaveChangesAsync();

            return await BuildInspectionReportDto(inspectionId);
        }

        [HttpGet("accounting")]
        public async Task<ActionResult<object>> GetAccounting()
        {
            var apartments = await _context.Apartments.ToListAsync();
            var totalRevenue = apartments.Sum(apartment => apartment.Rent);
            var occupied = apartments.Count(apartment => apartment.TenantID != null);
            var rentPaidPercent = apartments.Count == 0 ? 0 : (int)Math.Round((double)occupied / apartments.Count * 100);

            return new
            {
                totalRevenue,
                expenses = Math.Round(totalRevenue * 0.19m),
                rentPaidPercent,
                pendingInvoices = Math.Max(apartments.Count - occupied, 0)
            };
        }

        [HttpGet("accounting/history")]
        public async Task<ActionResult<IEnumerable<object>>> GetAccountingHistory()
        {
            var records = await _context.AccountingRecords
                .OrderBy(record => record.Year)
                .ThenBy(record => record.Month)
                .ToListAsync();

            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            return records.Select(record => new
            {
                record.Year,
                record.Month,
                label = $"{monthNames[record.Month - 1]} {record.Year}",
                record.Revenue,
                record.Expenses,
                profit = record.Revenue - record.Expenses
            }).ToList();
        }

        [HttpPost("support-messages")]
        public async Task<ActionResult<SupportMessage>> PostSupportMessage(SupportMessage message)
        {
            message.SupportMessageId = 0;
            message.SubmittedAt = DateTime.Now;
            _context.SupportMessages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(message);
        }

        [HttpGet("settings")]
        public async Task<ActionResult<AppSetting>> GetSettings()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSetting();
                _context.AppSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        [HttpPut("settings")]
        public async Task<ActionResult<AppSetting>> PutSettings(AppSetting updatedSettings)
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSetting();
                _context.AppSettings.Add(settings);
            }

            settings.DarkMode = updatedSettings.DarkMode;
            settings.EmailNotifications = updatedSettings.EmailNotifications;
            settings.AutoSaveReports = updatedSettings.AutoSaveReports;

            await _context.SaveChangesAsync();

            return settings;
        }

        private static object ProfileDto(AdminProfile profile) => new
        {
            profile.AdminProfileId,
            profile.FirstName,
            profile.LastName,
            profile.Email,
            profile.Phone,
            profile.Role,
            profile.Responsibilities,
            profile.ImageUrl
        };

        private async Task<AdminProfile> GetOrCreateProfileAsync()
        {
            var profile = await _context.AdminProfiles.FirstOrDefaultAsync();

            if (profile == null)
            {
                profile = new AdminProfile
                {
                    FirstName = "Viktor",
                    LastName = "Nilsson",
                    Email = "viktor@estatehub.com",
                    Phone = "070-123 45 67",
                    Role = "Administrator",
                    Responsibilities = "Full access to properties, tenants, billing, and system settings.",
                    PasswordHash = HashPassword("admin123"),
                    ImageUrl = "https://i.pravatar.cc/150?img=12"
                };
                _context.AdminProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return profile;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            var profile = await GetOrCreateProfileAsync();
            return ProfileDto(profile);
        }

        [HttpPut("profile")]
        public async Task<ActionResult<object>> PutProfile(AdminProfile updatedProfile)
        {
            var profile = await GetOrCreateProfileAsync();

            profile.FirstName = updatedProfile.FirstName;
            profile.LastName = updatedProfile.LastName;
            profile.Email = updatedProfile.Email;
            profile.Phone = updatedProfile.Phone;

            await _context.SaveChangesAsync();

            return ProfileDto(profile);
        }

        [HttpPost("profile/photo")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<ActionResult<object>> UploadProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only PNG, JPEG and GIF images are supported." });
            }

            var uploadsFolder = Path.Combine("wwwroot", "uploads", "profile");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var profile = await GetOrCreateProfileAsync();
            DeleteUploadedProfilePhoto(profile.ImageUrl);
            profile.ImageUrl = $"/uploads/profile/{fileName}";
            await _context.SaveChangesAsync();

            return ProfileDto(profile);
        }

        [HttpDelete("profile/photo")]
        public async Task<ActionResult<object>> RemoveProfilePhoto()
        {
            var profile = await GetOrCreateProfileAsync();
            DeleteUploadedProfilePhoto(profile.ImageUrl);
            profile.ImageUrl = "";
            await _context.SaveChangesAsync();

            return ProfileDto(profile);
        }

        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = "";
            public string NewPassword { get; set; } = "";
        }

        [HttpPost("profile/change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var profile = await _context.AdminProfiles.FirstOrDefaultAsync();

            if (profile == null || profile.PasswordHash != HashPassword(request.CurrentPassword))
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters." });
            }

            profile.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated." });
        }

        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginRequest request)
        {
            var profile = await _context.AdminProfiles.FirstOrDefaultAsync();

            if (profile == null
                || !string.Equals(profile.Email, request.Email, StringComparison.OrdinalIgnoreCase)
                || profile.PasswordHash != HashPassword(request.Password))
            {
                return Unauthorized(new { message = "Incorrect email or password." });
            }

            return Ok(ProfileDto(profile));
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        private static void DeleteUploadedProfilePhoto(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/profile/"))
            {
                return;
            }

            var filePath = Path.Combine("wwwroot", imageUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
