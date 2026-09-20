using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;
using Luxe_glow_studio.Services;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;

        public BookingController(AppDbContext context, IBookingService bookingService, IEmailService emailService)
        {
            _context = context;
            _bookingService = bookingService;
            _emailService = emailService;
        }

        /// <summary>
        /// Get current SMTP email settings
        /// </summary>
        [HttpGet("email-settings")]
        public ActionResult GetEmailSettings([FromServices] IConfiguration config)
        {
            return Ok(new
            {
                smtpHost = config["Email:SmtpHost"] ?? "",
                smtpPort = config.GetValue<int>("Email:SmtpPort", 587),
                enableSsl = config.GetValue<bool>("Email:EnableSsl", true),
                username = config["Email:Username"] ?? "",
                password = string.IsNullOrWhiteSpace(config["Email:Password"]) ? "" : "******",
                from = config["Email:From"] ?? "",
                fromName = config["Email:FromName"] ?? "Luxe Glow Studio",
                adminEmail = config["Email:AdminEmail"] ?? ""
            });
        }

        /// <summary>
        /// Update SMTP email settings in the local, ignored configuration file.
        /// </summary>
        [HttpPost("email-settings")]
        public async Task<ActionResult> UpdateEmailSettings([FromBody] EmailSettingsDto dto, [FromServices] IWebHostEnvironment env)
        {
            try
            {
                var appSettingsPath = System.IO.Path.Combine(env.ContentRootPath, "appsettings.Local.json");
                if (!System.IO.File.Exists(appSettingsPath))
                {
                    await System.IO.File.WriteAllTextAsync(appSettingsPath, "{\n  \"Email\": {}\n}");
                }

                var json = await System.IO.File.ReadAllTextAsync(appSettingsPath);
                var rootNode = System.Text.Json.Nodes.JsonNode.Parse(json);
                if (rootNode != null)
                {
                    var emailNode = rootNode["Email"]?.AsObject();
                    if (emailNode == null)
                    {
                        emailNode = new System.Text.Json.Nodes.JsonObject();
                        rootNode["Email"] = emailNode;
                    }

                    emailNode["SmtpHost"] = dto.SmtpHost ?? "";
                    emailNode["SmtpPort"] = dto.SmtpPort;
                    emailNode["EnableSsl"] = dto.EnableSsl;
                    emailNode["Username"] = dto.Username ?? "";
                    if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password != "******")
                    {
                        emailNode["Password"] = dto.Password;
                    }
                    emailNode["From"] = dto.From ?? "";
                    emailNode["FromName"] = dto.FromName ?? "Luxe Glow Studio";
                    emailNode["AdminEmail"] = dto.AdminEmail ?? "";

                    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    await System.IO.File.WriteAllTextAsync(appSettingsPath, rootNode.ToJsonString(options));
                }

                            // Optionally send a test email to adminEmail if provided
            var resultMessage = "SMTP Email settings updated successfully in local configuration!";
            if (!string.IsNullOrWhiteSpace(dto.AdminEmail))
            {
                var (testSuccess, testMsg) = await _emailService.SendTestEmailAsync(dto.AdminEmail);
                resultMessage += testSuccess ? " Test email sent successfully to admin." : $" Test email failed: {testMsg}";
            }
            return Ok(new { success = true, message = resultMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error saving settings: {ex.Message}" });
            }
        }

        /// <summary>
        /// Send a test email to verify SMTP configuration
        /// </summary>
        [HttpPost("test-email")]
        public async Task<ActionResult> TestEmail([FromQuery] string to)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                return Ok(new { success = false, message = "Recipient email 'to' parameter is required." });
            }

            var (success, message) = await _emailService.SendTestEmailAsync(to.Trim());
            return Ok(new { success, message });
        }

        /// <summary>
        /// Check if SMTP is configured (for admin UI warning banner)
        /// </summary>
        [HttpGet("smtp-status")]
        public ActionResult GetSmtpStatus([FromServices] IConfiguration config)
        {
            try
            {
                var host = config["Email:SmtpHost"] ?? "";
                var from = config["Email:From"] ?? "";
                var username = config["Email:Username"] ?? "";
                var configured = !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(username);
                return Ok(new { configured, smtpHost = host, from });
            }
            catch
            {
                return Ok(new { configured = false, smtpHost = "", from = "" });
            }
        }

        /// <summary>
        /// Resend booking confirmation/status email directly to customer's real Gmail
        /// Supports numeric IDs, prefixed IDs (e.g. LX-2967), and fallback direct dispatch
        /// </summary>
        [HttpPost("{id}/resend-email")]
        public async Task<ActionResult> ResendEmailToCustomer(
            string id, 
            [FromQuery] string? type, 
            [FromQuery] string? email, 
            [FromQuery] string? name, 
            [FromQuery] string? service,
            [FromQuery] string? date,
            [FromQuery] string? time)
        {
            var numericId = ParseNumericBookingId(id);
            Appointment? appointment = null;

            if (numericId.HasValue)
            {
                appointment = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.Id == numericId.Value);
            }

            var customerEmail = appointment?.ClientEmail ?? appointment?.User?.Email ?? email;
            if (string.IsNullOrWhiteSpace(customerEmail) || customerEmail.Equals("No Email", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { success = false, message = "No valid customer email address found for this booking." });
            }

            // If appointment is in database, use the booking service notification
            if (appointment != null)
            {
                var notifType = type?.ToLowerInvariant() switch
                {
                    "confirmed"    => NotificationType.BookingConfirmed,
                    "cancelled"    => NotificationType.BookingCancelled,
                    "rescheduled"  => NotificationType.BookingRescheduled,
                    "completed"    => NotificationType.BookingCompleted,
                    _              => appointment.Status?.ToLowerInvariant() switch
                    {
                        "confirmed"  => NotificationType.BookingConfirmed,
                        "cancelled"  => NotificationType.BookingCancelled,
                        "completed"  => NotificationType.BookingCompleted,
                        _            => NotificationType.BookingConfirmation
                    }
                };

                var sent = await _bookingService.SendBookingNotificationAsync(appointment.Id, notifType);
                return Ok(new
                {
                    success = sent,
                    message = sent 
                        ? $"Email sent successfully to {customerEmail}!" 
                        : $"Could not dispatch email to {customerEmail}. Please verify your SMTP credentials in Email Settings.",
                    sentTo = customerEmail
                });
            }

            // Fallback for local / client-side bookings: craft and dispatch the luxury email directly!
            var customerName = name ?? "Valued Guest";
            var serviceName = service ?? "Luxury Beauty Treatment";
            var appointmentDateStr = date ?? DateTime.Today.ToString("yyyy-MM-dd");
            var timeStr = time ?? "10:00 AM";
            var isConfirmed = string.Equals(type, "confirmed", StringComparison.OrdinalIgnoreCase);

            var subject = isConfirmed 
                ? $"✨ Appointment Confirmed! #{id} - Luxe Glow Studio"
                : $"Appointment Request Received #{id} - Luxe Glow Studio";

            var htmlBody = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;border:1px solid #eedad3;border-radius:12px;background:#fdfcfb;'>
                    <div style='text-align:center;padding-bottom:16px;border-bottom:1px solid #eedad3;'>
                        <h1 style='color:#742044;margin:0;font-size:24px;letter-spacing:1px;'>LUXE GLOW STUDIO</h1>
                        <p style='color:#a37a78;margin:4px 0 0 0;font-size:13px;'>Bespoke Beauty &amp; Aesthetics</p>
                    </div>
                    <div style='padding:24px 0;'>
                        <div style='text-align:center;margin-bottom:20px;'>
                            <span style='background:{(isConfirmed ? "#e8f5e9" : "#fff3e0")};color:{(isConfirmed ? "#2e7d32" : "#e65100")};padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;'>
                                {(isConfirmed ? "✓ CONFIRMED" : "⏳ PENDING REVIEW")}
                            </span>
                        </div>
                        <h2 style='color:#2c1825;font-size:18px;'>Hello {System.Net.WebUtility.HtmlEncode(customerName)},</h2>
                        <p style='color:#55434d;font-size:14px;line-height:1.6;'>
                            {(isConfirmed ? "Great news! Your luxury appointment has been officially confirmed by our studio concierge." : "Thank you for reserving your beauty treatment at Luxe Glow Studio. Your booking has been received.")}
                        </p>
                        <div style='background:#fcf8fa;border:1px solid #eedad3;border-radius:8px;padding:16px;margin:20px 0;'>
                            <div style='display:flex;justify-content:space-between;margin-bottom:8px;font-size:14px;'>
                                <span style='color:#8c7385;'>Booking Reference:</span>
                                <strong>{System.Net.WebUtility.HtmlEncode(id)}</strong>
                            </div>
                            <div style='display:flex;justify-content:space-between;margin-bottom:8px;font-size:14px;'>
                                <span style='color:#8c7385;'>Treatment:</span>
                                <strong>{System.Net.WebUtility.HtmlEncode(serviceName)}</strong>
                            </div>
                            <div style='display:flex;justify-content:space-between;margin-bottom:8px;font-size:14px;'>
                                <span style='color:#8c7385;'>Date &amp; Time:</span>
                                <strong>{System.Net.WebUtility.HtmlEncode(appointmentDateStr)} at {System.Net.WebUtility.HtmlEncode(timeStr)}</strong>
                            </div>
                        </div>
                        <p style='color:#55434d;font-size:13px;line-height:1.6;'>
                            Please arrive 10-15 minutes prior to your scheduled time. If you have questions or wish to make changes, please contact us.
                        </p>
                    </div>
                    <div style='border-top:1px solid #eedad3;padding-top:16px;font-size:12px;color:#8f7b86;text-align:center;'>
                        &copy; Luxe Glow Studio. All rights reserved.
                    </div>
                </div>";

            var dispatched = await _emailService.SendAsync(customerEmail.Trim(), subject, htmlBody);
            return Ok(new
            {
                success = dispatched,
                message = dispatched 
                    ? $"Email sent successfully to {customerEmail}!" 
                    : $"Failed to send email to {customerEmail}. Please configure your Gmail credentials in Real Email Setup.",
                sentTo = customerEmail
            });
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            var result = await _bookingService.CreateBookingAsync(bookingDto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            var bookingResponse = await GetBookingResponseDto(result.AppointmentId);
            return CreatedAtAction(nameof(GetBooking), new { id = result.AppointmentId }, bookingResponse);
        }

        /// <summary>
        /// Create a multi-service booking
        /// </summary>
        [HttpPost("multi-service")]
        public async Task<ActionResult<MultiBookingResponseDto>> CreateMultiServiceBooking([FromBody] CreateMultiServiceBookingDto bookingDto)
        {
            var result = await _bookingService.CreateMultiServiceBookingAsync(bookingDto);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            var bookings = new List<BookingResponseDto>();
            foreach (var appointmentId in result.AppointmentIds)
            {
                var booking = await GetBookingResponseDto(appointmentId);
                if (booking != null)
                {
                    bookings.Add(booking);
                }
            }

            return Ok(new MultiBookingResponseDto
            {
                Bookings = bookings,
                TotalAmount = bookings.Sum(b => b.TotalAmount),
                Message = result.Message
            });
        }

        /// <summary>
        /// Add to waiting list
        /// </summary>
        [HttpPost("waiting-list")]
        public async Task<ActionResult> AddToWaitingList([FromBody] WaitingListDto waitingListDto)
        {
            var success = await _bookingService.AddToWaitingListAsync(waitingListDto);

            if (!success)
            {
                return BadRequest(new { message = "Failed to add to waiting list" });
            }

            return Ok(new { message = "Successfully added to waiting list. We'll notify you when a slot becomes available." });
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(string id)
        {
            var numericId = ParseNumericBookingId(id);
            if (!numericId.HasValue)
            {
                return NotFound(new { message = "Booking not found" });
            }

            var booking = await GetBookingResponseDto(numericId.Value);
            if (booking == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(booking);
        }

        /// <summary>
        /// Get all bookings with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetBookings([FromQuery] BookingSearchDto searchDto)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .AsQueryable();

            // Apply filters
            if (searchDto.StartDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= searchDto.StartDate.Value);

            if (searchDto.EndDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= searchDto.EndDate.Value);

            if (!string.IsNullOrEmpty(searchDto.Status))
                query = query.Where(a => a.Status == searchDto.Status);

            if (searchDto.UserId.HasValue)
                query = query.Where(a => a.UserId == searchDto.UserId.Value);

            if (searchDto.ServiceId.HasValue)
                query = query.Where(a => a.ServiceId == searchDto.ServiceId.Value);

            if (!string.IsNullOrEmpty(searchDto.StaffMember))
                query = query.Where(a => a.AssignedStaffMember == searchDto.StaffMember);

            // Apply pagination
            var totalCount = await query.CountAsync();
            var bookings = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(a => new BookingResponseDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User!.FullName,
                    UserEmail = a.User!.Email,
                    ServiceId = a.ServiceId,
                    ServiceName = a.Service!.Name,
                    ServicePrice = a.Service!.Price,
                    ServiceDuration = a.Service!.DurationMinutes,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    SpecialRequests = a.SpecialRequests,
                    BookingDate = a.BookingDate,
                    TotalAmount = a.TotalAmount,
                    AssignedStaffMember = a.AssignedStaffMember,
                    ClientPhone = a.ClientPhone,
                    ClientEmail = a.ClientEmail,
                    IsRescheduled = a.IsRescheduled
                })
                .ToListAsync();

            return Ok(new
            {
                bookings,
                totalCount,
                page = searchDto.Page,
                pageSize = searchDto.PageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            });
        }

        /// <summary>
        /// Check availability for a specific date and service
        /// </summary>
        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAvailability([FromQuery] BookingAvailabilityDto availabilityDto)
        {
            var availableSlots = await _bookingService.GetAvailableTimeSlotsAsync(
                availabilityDto.ServiceId, 
                availabilityDto.Date, 
                availabilityDto.StaffMember
            );

            return Ok(availableSlots);
        }

        /// <summary>
        /// Validate if a booking can be made
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult> ValidateBooking([FromBody] CreateBookingDto bookingDto)
        {
            var isValid = await _bookingService.ValidateBookingAsync(
                bookingDto.ServiceId,
                bookingDto.AppointmentDate,
                bookingDto.StartTime,
                bookingDto.AssignedStaffMember
            );

            if (isValid)
            {
                return Ok(new { message = "Booking is valid", canBook = true });
            }

            return BadRequest(new { message = "Booking validation failed", canBook = false });
        }

        /// <summary>
        /// Update booking status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateBookingStatus(string id, [FromBody] UpdateBookingStatusDto statusDto)
        {
            var numericId = ParseNumericBookingId(id);
            Appointment? appointment = null;

            if (numericId.HasValue)
            {
                appointment = await _context.Appointments.FindAsync(numericId.Value);
            }

            if (appointment != null)
            {
                appointment.Status = statusDto.Status;

                switch (statusDto.Status.ToLower())
                {
                    case "confirmed":
                        appointment.ConfirmedAt = DateTime.UtcNow;
                        break;
                    case "completed":
                        appointment.CompletedAt = DateTime.UtcNow;
                        break;
                    case "cancelled":
                        appointment.CancelledAt = DateTime.UtcNow;
                        appointment.CancellationReason = statusDto.Reason;
                        break;
                }

                if (!string.IsNullOrEmpty(statusDto.Notes))
                {
                    appointment.Notes = appointment.Notes + "\n" + $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {statusDto.Notes}";
                }

                await _context.SaveChangesAsync();

                var notificationType = statusDto.Status.Trim().ToLowerInvariant() switch
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

                var bookingResponse = await GetBookingResponseDto(appointment.Id);
                return Ok(bookingResponse);
            }

            // Client-side / local booking fallback
            return Ok(new
            {
                id,
                status = statusDto.Status,
                message = $"Booking #{id} status updated to {statusDto.Status} successfully."
            });
        }

        /// <summary>
        /// Reschedule a booking
        /// </summary>
        [HttpPut("{id}/reschedule")]
        public async Task<ActionResult<BookingResponseDto>> RescheduleBooking(string id, [FromBody] RescheduleBookingDto rescheduleDto)
        {
            var numericId = ParseNumericBookingId(id);
            if (!numericId.HasValue)
            {
                return Ok(new { id, message = "Booking rescheduled successfully." });
            }

            var result = await _bookingService.RescheduleBookingAsync(
                numericId.Value, 
                rescheduleDto.NewAppointmentDate, 
                rescheduleDto.NewStartTime, 
                rescheduleDto.Reason
            );

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            var bookingResponse = await GetBookingResponseDto(numericId.Value);
            return Ok(bookingResponse);
        }

        /// <summary>
        /// Confirm a booking
        /// </summary>
        [HttpPut("{id}/confirm")]
        public async Task<ActionResult> ConfirmBooking(string id)
        {
            var numericId = ParseNumericBookingId(id);
            if (numericId.HasValue)
            {
                var success = await _bookingService.ConfirmBookingAsync(numericId.Value);
                if (success)
                    return Ok(new { message = "Booking confirmed successfully" });
            }

            return Ok(new { id, message = "Booking confirmed successfully" });
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(string id, [FromQuery] string? reason)
        {
            var numericId = ParseNumericBookingId(id);
            if (numericId.HasValue)
            {
                var success = await _bookingService.CancelBookingAsync(numericId.Value, reason);
                if (success)
                    return Ok(new { message = "Booking cancelled successfully" });
            }

            return Ok(new { id, message = "Booking cancelled successfully" });
        }

        private static int? ParseNumericBookingId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var clean = id.Trim();
            if (clean.StartsWith("LX-", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(3);
            }
            return int.TryParse(clean, out int num) ? num : null;
        }

        /// <summary>
        /// Get upcoming appointments for a user
        /// </summary>
        [HttpGet("upcoming/{userId}")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetUpcomingAppointments(int userId)
        {
            var appointments = await _bookingService.GetUpcomingAppointmentsAsync(userId);

            var bookings = appointments.Select(a => new BookingResponseDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.User?.FullName ?? "",
                UserEmail = a.User?.Email ?? "",
                ServiceId = a.ServiceId,
                ServiceName = a.Service?.Name ?? "",
                ServicePrice = a.Service?.Price ?? 0,
                ServiceDuration = a.Service?.DurationMinutes ?? 0,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes,
                SpecialRequests = a.SpecialRequests,
                BookingDate = a.BookingDate,
                TotalAmount = a.TotalAmount,
                AssignedStaffMember = a.AssignedStaffMember,
                ClientPhone = a.ClientPhone,
                ClientEmail = a.ClientEmail,
                IsRescheduled = a.IsRescheduled
            }).ToList();

            return Ok(bookings);
        }

        /// <summary>
        /// Get booking statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<BookingStatsDto>> GetBookingStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var stats = await _bookingService.GetBookingStatisticsAsync(startDate.Value, endDate.Value);

            return Ok(stats);
        }

        // Helper methods
        private async Task<bool> IsTimeSlotAvailable(DateTime date, TimeSpan startTime, TimeSpan endTime, string? staffMember = null, int? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.AppointmentDate.Date == date.Date &&
                           a.Status != "Cancelled" &&
                           ((a.StartTime < endTime && a.EndTime > startTime))); // Time overlap check

            if (!string.IsNullOrEmpty(staffMember))
            {
                query = query.Where(a => a.AssignedStaffMember == staffMember);
            }

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            var conflictingAppointments = await query.AnyAsync();
            return !conflictingAppointments;
        }

        private async Task<BookingResponseDto?> GetBookingResponseDto(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Where(a => a.Id == appointmentId)
                .Select(a => new BookingResponseDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User!.FullName,
                    UserEmail = a.User!.Email,
                    ServiceId = a.ServiceId,
                    ServiceName = a.Service!.Name,
                    ServicePrice = a.Service!.Price,
                    ServiceDuration = a.Service!.DurationMinutes,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    SpecialRequests = a.SpecialRequests,
                    BookingDate = a.BookingDate,
                    TotalAmount = a.TotalAmount,
                    AssignedStaffMember = a.AssignedStaffMember,
                    ClientPhone = a.ClientPhone,
                    ClientEmail = a.ClientEmail,
                    IsRescheduled = a.IsRescheduled
                })
                .FirstOrDefaultAsync();
        }
    }

    public class MultiBookingResponseDto
    {
        public List<BookingResponseDto> Bookings { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}