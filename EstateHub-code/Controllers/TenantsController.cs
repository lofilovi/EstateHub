using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstateHub_code.Data;
using EstateHub_code.Models;

namespace EstateHub_code.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TenantsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants()
        {
            return await _context.Tenants.ToListAsync();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Tenant>>> SearchTenants([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _context.Tenants.ToListAsync();
            }

            var search = query.Trim();
            return await _context.Tenants
                .Where(t =>
                    (t.PersonalNumber != null && t.PersonalNumber.Contains(search)) ||
                    (t.FirstName != null && t.FirstName.Contains(search)) ||
                    (t.LastName != null && t.LastName.Contains(search)) ||
                    ((t.FirstName ?? "") + " " + (t.LastName ?? "")).Contains(search))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Tenant>> PostTenant(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return Ok(tenant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTenant(int id, Tenant tenant)
        {
            if (id != tenant.TenantID) return BadRequest();

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tenants.Any(e => e.TenantID == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);

            if (tenant == null)
            {
                return NotFound();
            }

            var apartments = await _context.Apartments
                .Where(apartment => apartment.TenantID == id)
                .ToListAsync();

            foreach (var apartment in apartments)
            {
                apartment.TenantID = null;
                apartment.Status = "Ledig";
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
