using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;
using Microsoft.EntityFrameworkCore;

namespace Luxe_glow_studio.Services
{
    public interface IBookingService
    {
        Task<BookingResult> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<BookingResult> CreateMultiServiceBookingAsync(CreateMultiServiceBookingDto bookingDto);
        Task<bool> ValidateBookingAsync(int serviceId, DateTime date, TimeSpan startTime, string? staffMember = null);
        Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(int serviceId, DateTime date, string? staffMember = null);
        Task<BookingResult> RescheduleBookingAsync(int bookingId, DateTime newDate, TimeSpan newStartTime, string? reason = null);
        Task<bool> CancelBookingAsync(int bookingId, string? reason = null);
        Task<bool> ConfirmBookingAsync(int bookingId);
        Task<List<Appointment>> GetUpcomingAppointmentsAsync(int userId);
        Task<BookingStatsDto> GetBookingStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<bool> AddToWaitingListAsync(WaitingListDto waitingListDto);
        Task<bool> SendBookingNotificationAsync(int bookingId, NotificationType type);
    }

    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(AppDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BookingResult> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            try
            {
                // Validate user
                var user = await _context.Users.FindAsync(bookingDto.UserId);
                if (user == null)
                {
                    return BookingResult.Failure("User not found");
                }

                // Validate service
                var service = await _context.Services
                    .Include(s => s.Category)
                    .FirstOrDefaultAsync(s => s.Id == bookingDto.ServiceId && s.IsActive);
                
                if (service == null)
                {
                    return BookingResult.Failure("Service not found or not available");
                }

                // Calculate end time
                var endTime = bookingDto.StartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

                // Validate booking rules
                var validationResult = await ValidateBookingRulesAsync(service, bookingDto.AppointmentDate, bookingDto.StartTime);
                if (!validationResult.IsSuccess)
                {
                    return validationResult;
                }

                // Check availability
                var isAvailable = await IsTimeSlotAvailableAsync(
                    bookingDto.AppointmentDate, 
                    bookingDto.StartTime, 
                    endTime, 
                    bookingDto.AssignedStaffMember
                );

                if (!isAvailable)
                {
                    return BookingResult.Failure("Time slot is not available");
                }

                // Assign staff if not specified
                var assignedStaff = bookingDto.AssignedStaffMember;
                if (string.IsNullOrEmpty(assignedStaff))
                {
                    assignedStaff = await AssignBestAvailableStaffAsync(service.Id, bookingDto.AppointmentDate, bookingDto.StartTime, endTime);
                }

                // Calculate amount
                var amount = service.HasDiscount ? service.DiscountPrice!.Value : service.Price;
                if (bookingDto.IsHomeService && service.IsAvailableForHomeService)
                {
                    amount += service.HomeServiceExtraCharge;
                }

                // Create appointment
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
                    TotalAmount = amount,
                    AssignedStaffMember = assignedStaff,
                    ClientPhone = bookingDto.ClientPhone ?? user.PhoneNumber,
                    ClientEmail = bookingDto.ClientEmail ?? user.Email,
                    SendReminder = true
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Send confirmation notification
                await SendBookingNotificationAsync(appointment.Id, NotificationType.BookingConfirmation);

                _logger.LogInformation($"Booking created successfully. Appointment ID: {appointment.Id}");

                return BookingResult.Success(appointment.Id, "Booking created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return BookingResult.Failure("An error occurred while creating the booking");
            }
        }

        public async Task<BookingResult> CreateMultiServiceBookingAsync(CreateMultiServiceBookingDto bookingDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var appointmentIds = new List<int>();
                var currentStartTime = bookingDto.StartTime;
                var currentDate = bookingDto.AppointmentDate;

                foreach (var serviceId in bookingDto.ServiceIds)
                {
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service == null || !service.IsActive)
                    {
                        await transaction.RollbackAsync();
                        return BookingResult.Failure($"Service with ID {serviceId} not found or not available");
                    }

                    var endTime = currentStartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

                    // Check if we need to move to next day
                    if (endTime.TotalHours > 18) // After closing time
                    {
                        currentDate = currentDate.AddDays(1);
                        currentStartTime = new TimeSpan(9, 0, 0); // Start at opening time
                        endTime = currentStartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));
                    }

                    var isAvailable = await IsTimeSlotAvailableAsync(
                        currentDate,
                        currentStartTime,
                        endTime,
                        bookingDto.AssignedStaffMember
                    );

                    if (!isAvailable)
                    {
                        await transaction.RollbackAsync();
                        return BookingResult.Failure($"Time slot not available for service: {service.Name}");
                    }

                    var amount = service.HasDiscount ? service.DiscountPrice!.Value : service.Price;

                    var appointment = new Appointment
                    {
                        UserId = bookingDto.UserId,
                        ServiceId = serviceId,
                        AppointmentDate = currentDate,
                        StartTime = currentStartTime,
                        EndTime = endTime,
                        Status = "Pending",
                        Notes = bookingDto.Notes,
                        SpecialRequests = bookingDto.SpecialRequests,
                        BookingDate = DateTime.UtcNow,
                        TotalAmount = amount,
                        AssignedStaffMember = bookingDto.AssignedStaffMember,
                        ClientPhone = bookingDto.ClientPhone,
                        ClientEmail = bookingDto.ClientEmail,
                        SendReminder = true
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    appointmentIds.Add(appointment.Id);

                    // Update start time for next service
                    currentStartTime = endTime.Add(TimeSpan.FromMinutes(15)); // 15 min buffer
                }

                await transaction.CommitAsync();

                _logger.LogInformation($"Multi-service booking created. Appointment IDs: {string.Join(", ", appointmentIds)}");

                return BookingResult.Success(appointmentIds.First(), "Multi-service booking created successfully", appointmentIds);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating multi-service booking");
                return BookingResult.Failure("An error occurred while creating the multi-service booking");
            }
        }

        public async Task<bool> ValidateBookingAsync(int serviceId, DateTime date, TimeSpan startTime, string? staffMember = null)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null || !service.IsActive)
                return false;

            var validationResult = await ValidateBookingRulesAsync(service, date, startTime);
            return validationResult.IsSuccess;
        }

        public async Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(int serviceId, DateTime date, string? staffMember = null)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
                return new List<TimeSlotDto>();

            var slots = GenerateTimeSlots(date, service.DurationMinutes);
            var availableSlots = new List<TimeSlotDto>();

            foreach (var slot in slots)
            {
                var isAvailable = await IsTimeSlotAvailableAsync(date, slot.StartTime, slot.EndTime, staffMember);
                var validationResult = await ValidateBookingRulesAsync(service, date, slot.StartTime);

                availableSlots.Add(new TimeSlotDto
                {
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsAvailable = isAvailable && validationResult.IsSuccess,
                    UnavailableReason = !isAvailable ? "Already booked" : 
                                       !validationResult.IsSuccess ? validationResult.Message : null
                });
            }

            return availableSlots;
        }

        public async Task<BookingResult> RescheduleBookingAsync(int bookingId, DateTime newDate, TimeSpan newStartTime, string? reason = null)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == bookingId);

            if (appointment == null)
            {
                return BookingResult.Failure("Booking not found");
            }

            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
            {
                return BookingResult.Failure("Cannot reschedule completed or cancelled bookings");
            }

            var newEndTime = newStartTime.Add(TimeSpan.FromMinutes(appointment.Service!.DurationMinutes));

            var isAvailable = await IsTimeSlotAvailableAsync(newDate, newStartTime, newEndTime, appointment.AssignedStaffMember, bookingId);

            if (!isAvailable)
            {
                return BookingResult.Failure("New time slot is not available");
            }

            appointment.AppointmentDate = newDate;
            appointment.StartTime = newStartTime;
            appointment.EndTime = newEndTime;
            appointment.IsRescheduled = true;
            appointment.Status = "Pending";

            if (!string.IsNullOrEmpty(reason))
            {
                appointment.Notes += $"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Rescheduled: {reason}";
            }

            await _context.SaveChangesAsync();

            await SendBookingNotificationAsync(bookingId, NotificationType.BookingRescheduled);

            return BookingResult.Success(bookingId, "Booking rescheduled successfully");
        }

        public async Task<bool> CancelBookingAsync(int bookingId, string? reason = null)
        {
            var appointment = await _context.Appointments.FindAsync(bookingId);
            if (appointment == null)
                return false;

            appointment.Status = "Cancelled";
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.CancellationReason = reason;

            await _context.SaveChangesAsync();

            await SendBookingNotificationAsync(bookingId, NotificationType.BookingCancelled);

            // Check waiting list
            await ProcessWaitingListAsync(appointment.ServiceId, appointment.AppointmentDate);

            return true;
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId)
        {
            var appointment = await _context.Appointments.FindAsync(bookingId);
            if (appointment == null)
                return false;

            appointment.Status = "Confirmed";
            appointment.ConfirmedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await SendBookingNotificationAsync(bookingId, NotificationType.BookingConfirmed);

            return true;
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int userId)
        {
            return await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.UserId == userId && 
                           a.AppointmentDate >= DateTime.Today &&
                           a.Status != "Cancelled" &&
                           a.Status != "Completed")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<BookingStatsDto> GetBookingStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _context.Appointments
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .ToListAsync();

            var todayBookings = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == DateTime.Today)
                .ToListAsync();

            var weeklyStats = new List<DailyBookingStats>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayBookings = bookings.Where(a => a.AppointmentDate.Date == date.Date).ToList();
                weeklyStats.Add(new DailyBookingStats
                {
                    Date = date,
                    BookingCount = dayBookings.Count,
                    Revenue = dayBookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount)
                });
            }

            return new BookingStatsDto
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
        }

        public async Task<bool> AddToWaitingListAsync(WaitingListDto waitingListDto)
        {
            var waitingListEntry = new WaitingList
            {
                UserId = waitingListDto.UserId,
                ServiceId = waitingListDto.ServiceId,
                PreferredDate = waitingListDto.PreferredDate,
                PreferredTimeRange = waitingListDto.PreferredTimeRange,
                Notes = waitingListDto.Notes,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.WaitingLists.Add(waitingListEntry);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SendBookingNotificationAsync(int bookingId, NotificationType type)
        {
            // This would integrate with your notification service (email, SMS, push)
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == bookingId);

            if (appointment == null)
                return false;

            var notification = new Notification
            {
                UserId = appointment.UserId,
                Type = type.ToString(),
                Title = GetNotificationTitle(type),
                Message = GetNotificationMessage(type, appointment),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Notification sent for booking {bookingId}: {type}");

            return true;
        }

        // Private helper methods

        private async Task<BookingResult> ValidateBookingRulesAsync(Service service, DateTime date, TimeSpan startTime)
        {
            // Check if booking date is in the past
            var bookingDateTime = date.Date.Add(startTime);
            if (bookingDateTime <= DateTime.Now)
            {
                return BookingResult.Failure("Cannot book appointments in the past");
            }

            // Check minimum advance booking time
            var hoursUntilAppointment = (bookingDateTime - DateTime.Now).TotalHours;
            if (hoursUntilAppointment < service.MinAdvanceBookingHours)
            {
                return BookingResult.Failure($"Service requires at least {service.MinAdvanceBookingHours} hours advance booking");
            }

            // Check maximum advance booking time
            var daysUntilAppointment = (date.Date - DateTime.Today).Days;
            if (daysUntilAppointment > service.MaxAdvanceBookingDays)
            {
                return BookingResult.Failure($"Cannot book more than {service.MaxAdvanceBookingDays} days in advance");
            }

            // Check if within business hours
            if (startTime < new TimeSpan(9, 0, 0) || startTime >= new TimeSpan(18, 0, 0))
            {
                return BookingResult.Failure("Booking time must be between 9 AM and 6 PM");
            }

            return BookingResult.Success(0, "Validation passed");
        }

        private async Task<bool> IsTimeSlotAvailableAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, string? staffMember = null, int? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.AppointmentDate.Date == date.Date &&
                           a.Status != "Cancelled" &&
                           ((a.StartTime < endTime && a.EndTime > startTime)));

            if (!string.IsNullOrEmpty(staffMember))
            {
                query = query.Where(a => a.AssignedStaffMember == staffMember);
            }

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAppointmentId.Value);
            }

            return !await query.AnyAsync();
        }

        private List<TimeSlotDto> GenerateTimeSlots(DateTime date, int serviceDurationMinutes)
        {
            var slots = new List<TimeSlotDto>();
            var startHour = 9;
            var endHour = 18;
            var slotDuration = TimeSpan.FromMinutes(serviceDurationMinutes);

            for (var hour = startHour; hour < endHour; hour++)
            {
                for (var minute = 0; minute < 60; minute += 30)
                {
                    var startTime = new TimeSpan(hour, minute, 0);
                    var endTime = startTime.Add(slotDuration);

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

        private async Task<string?> AssignBestAvailableStaffAsync(int serviceId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Get all practitioners who can perform this service
            var practitioners = await _context.PractitionerServices
                .Include(ps => ps.Practitioner)
                .Where(ps => ps.ServiceId == serviceId && ps.IsActive && ps.Practitioner.IsActive)
                .Select(ps => ps.Practitioner.User.FullName)
                .ToListAsync();

            // Find first available practitioner
            foreach (var practitioner in practitioners)
            {
                var isAvailable = await IsTimeSlotAvailableAsync(date, startTime, endTime, practitioner);
                if (isAvailable)
                {
                    return practitioner;
                }
            }

            return null;
        }

        private async Task ProcessWaitingListAsync(int serviceId, DateTime date)
        {
            var waitingListEntries = await _context.WaitingLists
                .Include(w => w.User)
                .Where(w => w.ServiceId == serviceId && 
                           w.Status == "Active" &&
                           w.PreferredDate.Date == date.Date)
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

            foreach (var entry in waitingListEntries)
            {
                // Notify user about availability
                var notification = new Notification
                {
                    UserId = entry.UserId,
                    Type = "WaitingListAvailability",
                    Title = "Time Slot Available",
                    Message = $"A time slot is now available for {entry.Service?.Name} on {date:MMM dd, yyyy}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        private string GetNotificationTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.BookingConfirmation => "Booking Confirmation",
                NotificationType.BookingConfirmed => "Booking Confirmed",
                NotificationType.BookingRescheduled => "Booking Rescheduled",
                NotificationType.BookingCancelled => "Booking Cancelled",
                NotificationType.BookingReminder => "Upcoming Appointment Reminder",
                NotificationType.BookingCompleted => "Appointment Completed",
                _ => "Booking Notification"
            };
        }

        private string GetNotificationMessage(NotificationType type, Appointment appointment)
        {
            return type switch
            {
                NotificationType.BookingConfirmation => $"Your booking for {appointment.Service?.Name} has been created for {appointment.AppointmentDate:MMM dd, yyyy} at {appointment.StartTime}",
                NotificationType.BookingConfirmed => $"Your booking for {appointment.Service?.Name} on {appointment.AppointmentDate:MMM dd, yyyy} at {appointment.StartTime} has been confirmed",
                NotificationType.BookingRescheduled => $"Your booking has been rescheduled to {appointment.AppointmentDate:MMM dd, yyyy} at {appointment.StartTime}",
                NotificationType.BookingCancelled => $"Your booking for {appointment.Service?.Name} on {appointment.AppointmentDate:MMM dd, yyyy} has been cancelled",
                NotificationType.BookingReminder => $"Reminder: You have an appointment for {appointment.Service?.Name} tomorrow at {appointment.StartTime}",
                NotificationType.BookingCompleted => $"Thank you for visiting us! Your appointment for {appointment.Service?.Name} is complete",
                _ => "Booking update"
            };
        }
    }

    // Supporting classes
    public class BookingResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AppointmentId { get; set; }
        public List<int> AppointmentIds { get; set; } = new();

        public static BookingResult Success(int appointmentId, string message, List<int>? allIds = null)
        {
            return new BookingResult
            {
                IsSuccess = true,
                Message = message,
                AppointmentId = appointmentId,
                AppointmentIds = allIds ?? new List<int> { appointmentId }
            };
        }

        public static BookingResult Failure(string message)
        {
            return new BookingResult
            {
                IsSuccess = false,
                Message = message
            };
        }
    }

    public enum NotificationType
    {
        BookingConfirmation,
        BookingConfirmed,
        BookingRescheduled,
        BookingCancelled,
        BookingReminder,
        BookingCompleted
    }
}
