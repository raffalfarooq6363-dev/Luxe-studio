using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .ThenInclude(a => a!.Service)
                .ToListAsync();
        }

        // POST: api/payments
        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
        {
            // Verify ke Appointment exist karti hai ya nahi
            var appointmentExists = await _context.Appointments.AnyAsync(a => a.Id == payment.AppointmentId);
            if (!appointmentExists)
            {
                return BadRequest("Invalid AppointmentId. Appointment does not exist.");
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayments), new { id = payment.Id }, payment);
        }
    }
}