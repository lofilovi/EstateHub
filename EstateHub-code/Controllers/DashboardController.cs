using EstateHub_code.Data;
using EstateHub_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
