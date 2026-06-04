using EstateHub_code.Data;
using EstateHub_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
