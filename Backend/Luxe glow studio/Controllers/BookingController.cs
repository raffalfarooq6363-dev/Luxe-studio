using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(bookingDto.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Validate service exists
            var service = await _context.Services.FindAsync(bookingDto.ServiceId);
            if (service == null)
            {
                return BadRequest(new { message = "Service not found" });
            }

            // Calculate end time based on service duration
            var endTime = bookingDto.StartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

            // Check if time slot is available
            var isAvailable = await IsTimeSlotAvailable(
                bookingDto.AppointmentDate, 
                bookingDto.StartTime, 
                endTime, 
                bookingDto.AssignedStaffMember
            );

            if (!isAvailable)
            {
                return BadRequest(new { message = "Time slot is not available" });
            }

            var appointment = new Appointment
            {
                UserId = bookingDto.UserId,
                ServiceId = bookingDto.ServiceId,
                AppointmentDate = bookingDto.AppointmentDate,
                StartTime = bookingDto.StartTime,
                EndTime = endTime,
                Status = "Pending",
                Notes = bookingDto.Notes,
                SpecialRequests = bookingDto.SpecialRequests,
                BookingDate = DateTime.UtcNow,
                TotalAmount = service.Price,
                AssignedStaffMember = bookingDto.AssignedStaffMember,
                ClientPhone = bookingDto.ClientPhone ?? user.PhoneNumber,
                ClientEmail = bookingDto.ClientEmail ?? user.Email
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Return full booking details
            var bookingResponse = await GetBookingResponseDto(appointment.Id);
            return CreatedAtAction(nameof(GetBooking), new { id = appointment.Id }, bookingResponse);
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(int id)
        {
            var booking = await GetBookingResponseDto(id);
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
            var service = await _context.Services.FindAsync(availabilityDto.ServiceId);
            if (service == null)
            {
                return BadRequest(new { message = "Service not found" });
            }

            var timeSlots = GenerateTimeSlots(availabilityDto.Date, service.DurationMinutes);
            var availableSlots = new List<TimeSlotDto>();

            foreach (var slot in timeSlots)
            {
                var isAvailable = await IsTimeSlotAvailable(
                    availabilityDto.Date,
                    slot.StartTime,
                    slot.EndTime,
                    availabilityDto.StaffMember
                );

                availableSlots.Add(new TimeSlotDto
                {
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsAvailable = isAvailable,
                    UnavailableReason = isAvailable ? null : "Time slot is already booked"
                });
            }

            return Ok(availableSlots);
        }

        /// <summary>
        /// Update booking status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<BookingResponseDto>> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto statusDto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            var oldStatus = appointment.Status;
            appointment.Status = statusDto.Status;

            // Update timestamps based on status
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

            var bookingResponse = await GetBookingResponseDto(appointment.Id);
            return Ok(bookingResponse);
        }

        /// <summary>
        /// Reschedule a booking
        /// </summary>
        [HttpPut("{id}/reschedule")]
        public async Task<ActionResult<BookingResponseDto>> RescheduleBooking(int id, [FromBody] RescheduleBookingDto rescheduleDto)
        {
            var appointment = await _context.Appointments.Include(a => a.Service).FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                return BadRequest(new { message = "Cannot reschedule completed or cancelled bookings" });
            }

            var newEndTime = rescheduleDto.NewStartTime.Add(TimeSpan.FromMinutes(appointment.Service!.DurationMinutes));

            // Check if new time slot is available
            var isAvailable = await IsTimeSlotAvailable(
                rescheduleDto.NewAppointmentDate,
                rescheduleDto.NewStartTime,
                newEndTime,
                appointment.AssignedStaffMember,
                id // Exclude current appointment from conflict check
            );

            if (!isAvailable)
            {
                return BadRequest(new { message = "New time slot is not available" });
            }

            appointment.AppointmentDate = rescheduleDto.NewAppointmentDate;
            appointment.StartTime = rescheduleDto.NewStartTime;
            appointment.EndTime = newEndTime;
            appointment.IsRescheduled = true;
            appointment.Status = "Pending"; // Reset to pending after reschedule

            if (!string.IsNullOrEmpty(rescheduleDto.Reason))
            {
                appointment.Notes = appointment.Notes + "\n" + $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Rescheduled: {rescheduleDto.Reason}";
            }

            await _context.SaveChangesAsync();

            var bookingResponse = await GetBookingResponseDto(appointment.Id);
            return Ok(bookingResponse);
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id, [FromQuery] string? reason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = "Booking not found" });
            }

            appointment.Status = "Cancelled";
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.CancellationReason = reason;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking cancelled successfully" });
        }

        /// <summary>
        /// Get booking statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<BookingStatsDto>> GetBookingStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var bookings = await _context.Appointments
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .ToListAsync();

            var todayBookings = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == DateTime.Today)
                .ToListAsync();

            var weeklyStats = new List<DailyBookingStats>();
            for (var date = startDate.Value; date <= endDate.Value; date = date.AddDays(1))
            {
                var dayBookings = bookings.Where(a => a.AppointmentDate.Date == date.Date).ToList();
                weeklyStats.Add(new DailyBookingStats
                {
                    Date = date,
                    BookingCount = dayBookings.Count,
                    Revenue = dayBookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount)
                });
            }

            var stats = new BookingStatsDto
            {
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == "Pending"),
                ConfirmedBookings = bookings.Count(b => b.Status == "Confirmed"),
                CompletedBookings = bookings.Count(b => b.Status == "Completed"),
                CancelledBookings = bookings.Count(b => b.Status == "Cancelled"),
                TotalRevenue = bookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount),
                TodayRevenue = todayBookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount),
                TodayBookings = todayBookings.Count,
                WeeklyStats = weeklyStats
            };

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

        private List<TimeSlotDto> GenerateTimeSlots(DateTime date, int serviceDurationMinutes)
        {
            var slots = new List<TimeSlotDto>();
            var startHour = 9; // 9 AM
            var endHour = 18; // 6 PM
            var slotDuration = TimeSpan.FromMinutes(serviceDurationMinutes);

            for (var hour = startHour; hour < endHour; hour++)
            {
                for (var minute = 0; minute < 60; minute += 30) // 30-minute intervals
                {
                    var startTime = new TimeSpan(hour, minute, 0);
                    var endTime = startTime.Add(slotDuration);

                    // Don't create slots that go beyond closing time
                    if (endTime <= new TimeSpan(endHour, 0, 0))
                    {
                        slots.Add(new TimeSlotDto
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            IsAvailable = true
                        });
                    }
                }
            }

            return slots;
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
}