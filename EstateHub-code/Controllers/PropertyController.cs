using EstateHub_code.Data;
using EstateHub_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace EstateHub_code.Controllers
{
    [Route("api/[controller]")] // Detta gör att adressen blir /api/property
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PropertyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/property
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Property>>> GetProperties()
        {
            // Här hämtar vi allt från tabellen Property i MySQL!
            return await _context.Properties.ToListAsync();
        }
    }
}