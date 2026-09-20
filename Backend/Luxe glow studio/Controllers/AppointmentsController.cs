using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;
using Luxe_glow_studio.Services;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;

        public AppointmentsController(AppDbContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/appointments/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetUserAppointments(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        // GET: api/appointments/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetMyAppointments([FromQuery] int? userId)
        {
            int targetUserId = 0;
            if (userId.HasValue && userId.Value > 0)
            {
                targetUserId = userId.Value;
            }
            else
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out var parsed))
                {
                    targetUserId = parsed;
                }
            }

            if (targetUserId == 0)
            {
                return BadRequest(new { message = "UserId is required or user must be authenticated." });
            }

            return await _context.Appointments
                .Where(a => a.UserId == targetUserId)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] Appointment appointment)
        {
            // Verify if User exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == appointment.UserId);
            if (!userExists)
            {
                return BadRequest(new { message = "Invalid UserId. User does not exist." });
            }

            // Verify if Service exists
            var serviceExists = await _context.Services.AnyAsync(s => s.Id == appointment.ServiceId);
            if (!serviceExists)
            {
                return BadRequest(new { message = "Invalid ServiceId. Service does not exist." });
            }

            if (string.IsNullOrWhiteSpace(appointment.Status))
            {
                appointment.Status = "Pending";
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var loaded = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            return CreatedAtAction(nameof(GetAppointments), new { id = appointment.Id }, loaded ?? appointment);
        }

        public class StatusUpdateDto
        {
            public string Status { get; set; } = string.Empty;
        }

        // PUT: api/appointments/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound(new { message = $"Appointment with ID {id} not found." });
            }

            appointment.Status = dto.Status.Trim();
            if (dto.Status.Trim().Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                appointment.ConfirmedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            var notificationType = dto.Status.Trim().ToLowerInvariant() switch
            {
                "confirmed" => NotificationType.BookingConfirmed,
                "cancelled" or "rejected" => NotificationType.BookingCancelled,
                "completed" => NotificationType.BookingCompleted,
                "pending" => NotificationType.BookingConfirmation,
                _ => (NotificationType?)null
            };
            if (notificationType.HasValue)
            {
                await _bookingService.SendBookingNotificationAsync(appointment.Id, notificationType.Value);
            }

            return Ok(appointment);
        }

        // DELETE: api/appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = $"Appointment with ID {id} not found." });
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Appointment #{id} deleted successfully." });
        }
    }
}