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

        public BookingController(AppDbContext context, IBookingService bookingService)
        {
            _context = context;
            _bookingService = bookingService;
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
            var result = await _bookingService.RescheduleBookingAsync(
                id, 
                rescheduleDto.NewAppointmentDate, 
                rescheduleDto.NewStartTime, 
                rescheduleDto.Reason
            );

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            var bookingResponse = await GetBookingResponseDto(id);
            return Ok(bookingResponse);
        }

        /// <summary>
        /// Confirm a booking
        /// </summary>
        [HttpPut("{id}/confirm")]
        public async Task<ActionResult> ConfirmBooking(int id)
        {
            var success = await _bookingService.ConfirmBookingAsync(id);

            if (!success)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(new { message = "Booking confirmed successfully" });
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id, [FromQuery] string? reason)
        {
            var success = await _bookingService.CancelBookingAsync(id, reason);

            if (!success)
            {
                return NotFound(new { message = "Booking not found" });
            }

            return Ok(new { message = "Booking cancelled successfully" });
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