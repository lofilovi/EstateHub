using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstateHub_code.Data;
using EstateHub_code.Models;

namespace EstateHub_code.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/apartments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Apartment>>> GetApartments()
        {
            // .Include gör en SQL JOIN i bakgrunden helt automatiskt!
            return await _context.Apartments
                .Include(a => a.Property)
                .ToListAsync();
        }

        // GET: api/apartments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Apartment>> GetApartment(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);

            if (apartment == null)
            {
                return NotFound();
            }

            return apartment;
        }

        // POST: api/apartments
        [HttpPost]
        public async Task<ActionResult<Apartment>> PostApartment(Apartment apartment)
        {
            _context.Apartments.Add(apartment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetApartment), new { id = apartment.ApartmentID }, apartment);
        }
    }
}