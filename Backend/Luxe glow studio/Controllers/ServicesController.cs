using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            // Seed default services if table is empty
            if (!await _context.Services.AnyAsync())
            {
                var defaultServices = new List<Service>
                {
                    new Service
                    {
                        Name = "Signature Hydrafacial Glow",
                        Description = "Deep dermal cleansing, antioxidant infusion & LED therapy for radiant skin.",
                        Price = 120.00m,
                        DurationMinutes = 60
                    },
                    new Service
                    {
                        Name = "Haute Bridal & Occasion Styling",
                        Description = "Bespoke beauty styling, HD airbrush artistry & luxury skin prep.",
                        Price = 250.00m,
                        DurationMinutes = 120
                    },
                    new Service
                    {
                        Name = "Collagen Infusion & Peeling",
                        Description = "Advanced skin resurfacing restoring firmness, clarity & natural collagen vibrancy.",
                        Price = 160.00m,
                        DurationMinutes = 75
                    },
                    new Service
                    {
                        Name = "Luxe Spa Manicure & Pedicure",
                        Description = "Herbal foot bath, botanical scrub, paraffin wax treatment & gel finish.",
                        Price = 80.00m,
                        DurationMinutes = 45
                    }
                };

                _context.Services.AddRange(defaultServices);
                await _context.SaveChangesAsync();
            }

            return await _context.Services.ToListAsync();
        }

        // GET: api/services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound(new { message = $"Service with ID {id} not found." });
            }

            return service;
        }

        // POST: api/services
        [HttpPost]
        public async Task<ActionResult<Service>> CreateService([FromBody] Service service)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
        }

        // PUT: api/services/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] Service service)
        {
            if (id != service.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var existing = await _context.Services.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Service with ID {id} not found." });
            }

            existing.Name = service.Name;
            existing.Description = service.Description;
            existing.Price = service.Price;
            existing.DurationMinutes = service.DurationMinutes;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Services.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return Ok(existing);
        }

        // DELETE: api/services/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound(new { message = $"Service with ID {id} not found." });
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Service '{service.Name}' deleted successfully." });
        }
    }
}
