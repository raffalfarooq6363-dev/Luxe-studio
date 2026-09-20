using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyNotifications([FromQuery] int? userId, [FromQuery] string? email)
        {
            int targetUserId = 0;
            if (userId.HasValue && userId.Value > 0)
            {
                targetUserId = userId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
                if (foundUser != null) targetUserId = foundUser.Id;
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
                return Ok(new List<object>());
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == targetUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.UserId,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Priority,
                    n.IsRead,
                    n.ReadAt,
                    n.AppointmentId,
                    n.TemplateData,
                    n.CreatedAt
                })
                .Take(50)
                .ToListAsync();

            return Ok(notifications);
        }

        // PUT: api/notifications/5/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound(new { message = "Notification not found." });
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, id });
        }

        // PUT: api/notifications/mark-all-read
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int? userId)
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
                return BadRequest(new { message = "UserId is required." });
            }

            var unread = await _context.Notifications
                .Where(n => n.UserId == targetUserId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, markedCount = unread.Count });
        }

        // DELETE: api/notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound(new { message = "Notification not found." });
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Notification deleted." });
        }

        public class SendCustomNotificationDto
        {
            public int? UserId { get; set; }
            public string? UserEmail { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        // POST: api/notifications/send
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendCustomNotificationDto dto)
        {
            int targetUserId = 0;
            if (dto.UserId.HasValue && dto.UserId.Value > 0)
            {
                targetUserId = dto.UserId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(dto.UserEmail))
            {
                var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.UserEmail.Trim().ToLower());
                if (foundUser != null) targetUserId = foundUser.Id;
            }

            if (targetUserId == 0)
            {
                return BadRequest(new { message = "Valid recipient UserId or UserEmail is required." });
            }

            var notification = new Luxe_glow_studio.Models.Notification
            {
                UserId = targetUserId,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? "Message from Luxe Glow Studio" : dto.Title.Trim(),
                Message = dto.Message.Trim(),
                Type = "StudioMessage",
                Priority = "Medium",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                SendEmail = true,
                SendInApp = true,
                TemplateData = $@"<div style=""font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 25px; background: #fff5f8; border: 1px solid #ffd1dc; border-radius: 12px; color: #2d1822;""><div style=""text-align: center; border-bottom: 2px solid #dfb0bd; padding-bottom: 15px; margin-bottom: 20px;""><h2 style=""margin:0; color: #782944; letter-spacing: 2px;"">LUXE GLOW STUDIO</h2><p style=""margin: 5px 0 0; color: #9c6877; font-size: 13px; text-transform: uppercase;"">Personal Concierge Message</p></div><h3 style=""color: #782944; margin-top: 0;"">{dto.Title}</h3><div style=""line-height: 1.7; font-size: 15px; color: #3c1e2d; white-space: pre-line;"">{dto.Message}</div><div style=""margin-top: 25px; padding-top: 15px; border-top: 1px solid #f2cfd8; font-size: 12px; color: #8a6a75; text-align: center;""><p>Warmest regards,<br/><strong>The Luxe Glow Studio Concierge Team</strong><br/>124 Luxury Boulevard, Suite 400 • concierge@luxeglowstudio.com</p></div></div>"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, notificationId = notification.Id, message = "Message delivered to customer." });
        }
    }
}
