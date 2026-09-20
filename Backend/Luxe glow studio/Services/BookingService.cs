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
        Task<bool> NotifyAdminNewBookingAsync(int appointmentId);
    }

    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookingService> _logger;
        private readonly IEmailService _emailService;

        public BookingService(AppDbContext context, ILogger<BookingService> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        private async Task<User?> ResolveOrCreateCustomerUserAsync(int? userId, string? customerName, string? email, string? phone)
        {
            if (userId.HasValue && userId.Value > 0)
            {
                var existingUser = await _context.Users.FindAsync(userId.Value);
                if (existingUser != null)
                    return existingUser;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var cleanEmail = email.Trim().ToLowerInvariant();
                var existingByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);
                if (existingByEmail != null)
                {
                    if (!string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(existingByEmail.PhoneNumber))
                    {
                        existingByEmail.PhoneNumber = phone;
                        await _context.SaveChangesAsync();
                    }
                    return existingByEmail;
                }

                var rawName = string.IsNullOrWhiteSpace(customerName) ? "Valued Customer" : customerName.Trim();
                var parts = rawName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var firstName = parts.Length > 0 ? parts[0] : "Valued";
                var lastName = parts.Length > 1 ? parts[1] : "Customer";

                var newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = cleanEmail,
                    PhoneNumber = phone,
                    Role = "Customer",
                    PasswordHash = "GUEST_ACCOUNT:" + Guid.NewGuid().ToString("N"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return newUser;
            }

            return null;
        }

        public async Task<BookingResult> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            try
            {
                // Validate / resolve user
                var user = await ResolveOrCreateCustomerUserAsync(
                    bookingDto.UserId, 
                    bookingDto.CustomerName, 
                    bookingDto.ClientEmail, 
                    bookingDto.ClientPhone);

                if (user == null)
                {
                    return BookingResult.Failure("Valid customer details (user account or email) are required.");
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
                    UserId = user.Id,
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

                // 1. Send acknowledgement notification to customer
                await SendBookingNotificationAsync(appointment.Id, NotificationType.BookingConfirmation);

                // 2. Send notification to admin(s)
                await NotifyAdminNewBookingAsync(appointment.Id);

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
                var user = await ResolveOrCreateCustomerUserAsync(
                    bookingDto.UserId,
                    bookingDto.CustomerName,
                    bookingDto.ClientEmail,
                    bookingDto.ClientPhone);

                if (user == null)
                {
                    await transaction.RollbackAsync();
                    return BookingResult.Failure("Valid customer details (user account or email) are required.");
                }

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
                        UserId = user.Id,
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
                        ClientPhone = bookingDto.ClientPhone ?? user.PhoneNumber,
                        ClientEmail = bookingDto.ClientEmail ?? user.Email,
                        SendReminder = true
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    appointmentIds.Add(appointment.Id);

                    // Update start time for next service
                    currentStartTime = endTime.Add(TimeSpan.FromMinutes(15)); // 15 min buffer
                }

                await transaction.CommitAsync();

                // Send notifications for all created bookings
                foreach (var id in appointmentIds)
                {
                    await SendBookingNotificationAsync(id, NotificationType.BookingConfirmation);
                    await NotifyAdminNewBookingAsync(id);
                }

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

            var htmlBody = BuildBookingEmail(type, appointment);

            var notification = new Notification
            {
                UserId = appointment.UserId,
                Type = type.ToString(),
                Title = GetNotificationTitle(type),
                Message = GetNotificationMessage(type, appointment),
                TemplateData = htmlBody,
                AppointmentId = appointment.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(appointment.ClientEmail ?? appointment.User?.Email))
            {
                var emailSent = await _emailService.SendAsync(
                    appointment.ClientEmail ?? appointment.User!.Email,
                    notification.Title,
                    htmlBody);
                notification.EmailSent = emailSent;
                notification.EmailSentAt = emailSent ? DateTime.UtcNow : null;
                notification.EmailError = emailSent ? null : "Email delivery is not configured or failed.";
                await _context.SaveChangesAsync();
            }

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

        public async Task<bool> NotifyAdminNewBookingAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.User)
                    .Include(a => a.Service)
                    .ThenInclude(s => s!.Category)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                    return false;

                var customerName = appointment.User?.FullName ?? appointment.ClientEmail ?? "Customer";
                var serviceName = appointment.Service?.Name ?? "Beauty Treatment";
                var subject = $"✨ New Booking Request #{appointment.Id} - {serviceName} ({customerName})";
                var htmlBody = BuildAdminNewBookingEmail(appointment);

                // Save in-app notification for active Admin users
                try
                {
                    var adminUsers = await _context.Users
                        .Where(u => u.Role == "Admin" && u.IsActive)
                        .ToListAsync();

                    foreach (var admin in adminUsers)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = admin.Id,
                            Type = "AdminBookingAlert",
                            Title = subject,
                            Message = $"New booking #{appointment.Id} for {serviceName} by {customerName} on {appointment.AppointmentDate:MMM dd, yyyy} at {appointment.StartTime}",
                            TemplateData = htmlBody,
                            AppointmentId = appointment.Id,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    if (adminUsers.Count > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception exDb)
                {
                    _logger.LogWarning(exDb, "Could not save admin in-app notification to database.");
                }

                return await _emailService.SendEmailToAdminsAsync(subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new booking notification to admin for appointment {AppointmentId}.", appointmentId);
                return false;
            }
        }

        private string GetNotificationTitle(NotificationType type)
        {
            return type switch
            {
                NotificationType.BookingConfirmation => "Booking Request Received - Luxe Glow Studio",
                NotificationType.BookingConfirmed => "✨ Your Appointment is Confirmed! - Luxe Glow Studio",
                NotificationType.BookingRescheduled => "Appointment Rescheduled - Luxe Glow Studio",
                NotificationType.BookingCancelled => "Booking Status Update - Luxe Glow Studio",
                NotificationType.BookingReminder => "Appointment Reminder - Luxe Glow Studio",
                NotificationType.BookingCompleted => "Thank You for Visiting - Luxe Glow Studio",
                _ => "Booking Notification - Luxe Glow Studio"
            };
        }

        private string GetNotificationMessage(NotificationType type, Appointment appointment)
        {
            var serviceName = appointment.Service?.Name ?? "Beauty Treatment";
            var dateStr = appointment.AppointmentDate.ToString("dddd, MMMM dd, yyyy");
            var timeStr = appointment.StartTime.ToString(@"hh\:mm");

            return type switch
            {
                NotificationType.BookingConfirmation => $"We have received your reservation request for {serviceName} on {dateStr} at {timeStr}. Our team is reviewing the slot, and you will receive an official confirmation shortly.",
                NotificationType.BookingConfirmed => $"Great news! Your luxury appointment for {serviceName} on {dateStr} at {timeStr} has been officially confirmed. We look forward to welcoming you.",
                NotificationType.BookingRescheduled => $"Your appointment for {serviceName} has been rescheduled to {dateStr} at {timeStr}.",
                NotificationType.BookingCancelled => $"Your appointment for {serviceName} on {dateStr} could not be confirmed. Reason: {appointment.CancellationReason ?? "The studio could not accommodate this requested time slot."}",
                NotificationType.BookingReminder => $"Friendly reminder: You have an upcoming appointment for {serviceName} tomorrow at {timeStr}.",
                NotificationType.BookingCompleted => $"Thank you for visiting Luxe Glow Studio today! Your appointment for {serviceName} is complete. We hope you enjoyed your luxury experience.",
                _ => "Your booking status has been updated."
            };
        }

        private string BuildBookingEmail(NotificationType type, Appointment appointment)
        {
            var title = GetNotificationTitle(type);
            var message = GetNotificationMessage(type, appointment);
            var customerName = System.Net.WebUtility.HtmlEncode(appointment.User?.FirstName ?? "Valued Guest");
            var serviceName = System.Net.WebUtility.HtmlEncode(appointment.Service?.Name ?? "Beauty Service");
            var categoryName = System.Net.WebUtility.HtmlEncode(appointment.Service?.Category?.Name ?? "Aesthetics");
            var dateStr = appointment.AppointmentDate.ToString("dddd, MMMM dd, yyyy");
            var timeStr = appointment.StartTime.ToString(@"hh\:mm");
            var specialist = System.Net.WebUtility.HtmlEncode(appointment.AssignedStaffMember ?? "Senior Aesthetic Specialist");
            var amount = appointment.TotalAmount.ToString("N2");

            string statusBadgeHtml = type switch
            {
                NotificationType.BookingConfirmed => "<span style='background:#e8f5e9;color:#2e7d32;padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;letter-spacing:0.5px;'>✓ CONFIRMED</span>",
                NotificationType.BookingConfirmation => "<span style='background:#fff3e0;color:#e65100;padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;letter-spacing:0.5px;'>⏳ PENDING REVIEW</span>",
                NotificationType.BookingCancelled => "<span style='background:#ffebee;color:#c62828;padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;letter-spacing:0.5px;'>✕ CANCELLED</span>",
                NotificationType.BookingRescheduled => "<span style='background:#e1f5fe;color:#0277bd;padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;letter-spacing:0.5px;'>🗓 RESCHEDULED</span>",
                NotificationType.BookingCompleted => "<span style='background:#f3e5f5;color:#6a1b9a;padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;letter-spacing:0.5px;'>★ COMPLETED</span>",
                _ => "<span style='background:#eee;color:#333;padding:6px 16px;border-radius:20px;font-weight:bold;font-size:13px;'>UPDATE</span>"
            };

            string extraSectionHtml = "";
            if (type == NotificationType.BookingConfirmed)
            {
                extraSectionHtml = @"
                    <div style='background:#f9f5f6;border-left:4px solid #742044;padding:16px;border-radius:8px;margin-top:24px;'>
                        <h4 style='color:#742044;margin:0 0 8px 0;font-size:15px;'>Studio Visit Guidelines:</h4>
                        <ul style='margin:0;padding-left:20px;color:#55434d;font-size:13px;line-height:1.6;'>
                            <li>Please arrive 10-15 minutes early to unwind and complete any consultation forms.</li>
                            <li>Enjoy a complimentary herbal beverage or sparkling water upon arrival.</li>
                            <li>Need to reschedule? Please notify us at least 24 hours in advance.</li>
                        </ul>
                    </div>
                    <div style='margin-top:20px;padding:16px;background:#fff;border:1px solid #eedad3;border-radius:8px;font-size:13px;color:#55434d;'>
                        <strong style='color:#2c1825;'>Studio Location:</strong> Luxe Glow Studio, Main Boulevard, Suite 400<br/>
                        <strong style='color:#2c1825;'>Need Assistance?</strong> Call us at +1 (555) 234-5678 or reply to this email.
                    </div>";
            }
            else if (type == NotificationType.BookingConfirmation)
            {
                extraSectionHtml = @"
                    <div style='background:#fffbf2;border-left:4px solid #e65100;padding:16px;border-radius:8px;margin-top:24px;'>
                        <h4 style='color:#e65100;margin:0 0 6px 0;font-size:15px;'>What Happens Next?</h4>
                        <p style='margin:0;color:#55434d;font-size:13px;line-height:1.6;'>
                            Our studio manager has received your appointment request. Once the admin approves and confirms your time slot, you will receive an official confirmation email.
                        </p>
                    </div>";
            }
            else if (type == NotificationType.BookingCancelled && !string.IsNullOrWhiteSpace(appointment.CancellationReason))
            {
                extraSectionHtml = $@"
                    <div style='background:#fff5f5;border-left:4px solid #c62828;padding:16px;border-radius:8px;margin-top:24px;'>
                        <h4 style='color:#c62828;margin:0 0 6px 0;font-size:15px;'>Reason for Cancellation:</h4>
                        <p style='margin:0;color:#55434d;font-size:13px;line-height:1.6;'>
                            {System.Net.WebUtility.HtmlEncode(appointment.CancellationReason)}
                        </p>
                    </div>";
            }

            return $@"
            <div style='font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;background-color:#f7f4f2;padding:32px 16px;color:#2c1825;'>
                <div style='max-width:600px;margin:0 auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 8px 30px rgba(116,32,68,0.08);border:1px solid #eedad3;'>
                    
                    <!-- Header -->
                    <div style='background:linear-gradient(135deg, #742044 0%, #4a1029 100%);padding:36px 24px;text-align:center;'>
                        <h1 style='color:#ffffff;margin:0;font-size:26px;letter-spacing:3px;font-weight:700;text-transform:uppercase;'>LUXE GLOW STUDIO</h1>
                        <p style='color:#f2d2dc;margin:6px 0 0 0;font-size:13px;letter-spacing:1px;text-transform:uppercase;'>Luxury Beauty &amp; Aesthetics</p>
                    </div>

                    <!-- Body Content -->
                    <div style='padding:32px 28px;'>
                        <div style='display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;'>
                            <h2 style='color:#2c1825;font-size:20px;margin:0;'>Hello, {customerName}!</h2>
                            <div>{statusBadgeHtml}</div>
                        </div>

                        <p style='color:#55434d;font-size:15px;line-height:1.6;margin:0 0 24px 0;'>
                            {System.Net.WebUtility.HtmlEncode(message)}
                        </p>

                        <!-- Reservation Summary Card -->
                        <div style='background:#faf7f6;border-radius:12px;border:1px solid #eedad3;padding:20px;'>
                            <h3 style='color:#742044;margin:0 0 16px 0;font-size:16px;border-bottom:1px solid #eedad3;padding-bottom:8px;letter-spacing:0.5px;'>
                                Reservation Details (Ref: #{appointment.Id})
                            </h3>
                            
                            <table style='width:100%;border-collapse:collapse;font-size:14px;color:#2c1825;'>
                                <tr>
                                    <td style='padding:8px 0;color:#7a6b73;'>Service:</td>
                                    <td style='padding:8px 0;text-align:right;font-weight:600;'>{serviceName}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px 0;color:#7a6b73;'>Category:</td>
                                    <td style='padding:8px 0;text-align:right;font-weight:600;'>{categoryName}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px 0;color:#7a6b73;'>Date:</td>
                                    <td style='padding:8px 0;text-align:right;font-weight:600;'>{dateStr}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px 0;color:#7a6b73;'>Time:</td>
                                    <td style='padding:8px 0;text-align:right;font-weight:600;'>{timeStr}</td>
                                </tr>
                                <tr>
                                    <td style='padding:8px 0;color:#7a6b73;'>Specialist:</td>
                                    <td style='padding:8px 0;text-align:right;font-weight:600;'>{specialist}</td>
                                </tr>
                                <tr>
                                    <td style='padding:12px 0 4px 0;border-top:1px dashed #eedad3;color:#742044;font-weight:bold;'>Total Amount:</td>
                                    <td style='padding:12px 0 4px 0;border-top:1px dashed #eedad3;text-align:right;font-size:18px;font-weight:bold;color:#742044;'>${amount}</td>
                                </tr>
                            </table>
                        </div>

                        {extraSectionHtml}
                    </div>

                    <!-- Footer -->
                    <div style='background:#f9f6f5;padding:24px;border-top:1px solid #eedad3;text-align:center;font-size:12px;color:#8f7b86;'>
                        <p style='margin:0 0 6px 0;font-weight:600;color:#2c1825;'>Luxe Glow Studio</p>
                        <p style='margin:0 0 12px 0;'>Luxury Wellness &amp; Aesthetic Treatments</p>
                        <p style='margin:0;color:#a896a0;'>&copy; {DateTime.UtcNow.Year} Luxe Glow Studio. All rights reserved.</p>
                    </div>

                </div>
            </div>";
        }

        private string BuildAdminNewBookingEmail(Appointment appointment)
        {
            var customerName = System.Net.WebUtility.HtmlEncode(appointment.User?.FullName ?? appointment.ClientEmail ?? "Guest Customer");
            var customerEmail = System.Net.WebUtility.HtmlEncode(appointment.ClientEmail ?? appointment.User?.Email ?? "Not provided");
            var customerPhone = System.Net.WebUtility.HtmlEncode(appointment.ClientPhone ?? appointment.User?.PhoneNumber ?? "Not provided");
            var serviceName = System.Net.WebUtility.HtmlEncode(appointment.Service?.Name ?? "Beauty Service");
            var categoryName = System.Net.WebUtility.HtmlEncode(appointment.Service?.Category?.Name ?? "General");
            var dateStr = appointment.AppointmentDate.ToString("dddd, MMMM dd, yyyy");
            var timeStr = appointment.StartTime.ToString(@"hh\:mm");
            var specialist = System.Net.WebUtility.HtmlEncode(appointment.AssignedStaffMember ?? "First Available Specialist");
            var amount = appointment.TotalAmount.ToString("N2");
            var notes = string.IsNullOrWhiteSpace(appointment.Notes) ? "None" : System.Net.WebUtility.HtmlEncode(appointment.Notes);
            var specialRequests = string.IsNullOrWhiteSpace(appointment.SpecialRequests) ? "None" : System.Net.WebUtility.HtmlEncode(appointment.SpecialRequests);

            return $@"
            <div style='font-family:-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;background-color:#1e141a;padding:32px 16px;color:#2c1825;'>
                <div style='max-width:600px;margin:0 auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 12px 40px rgba(0,0,0,0.3);'>
                    
                    <!-- Admin Header -->
                    <div style='background:linear-gradient(135deg, #2c1825 0%, #150912 100%);padding:30px 24px;border-bottom:3px solid #d4af37;'>
                        <div style='display:flex;justify-content:space-between;align-items:center;'>
                            <div>
                                <h1 style='color:#ffffff;margin:0;font-size:22px;letter-spacing:2px;font-weight:700;'>LUXE GLOW STUDIO</h1>
                                <p style='color:#d4af37;margin:4px 0 0 0;font-size:12px;letter-spacing:1px;text-transform:uppercase;font-weight:600;'>Admin Notification System</p>
                            </div>
                            <div style='text-align:right;'>
                                <span style='background:#d4af37;color:#1e141a;padding:5px 12px;border-radius:12px;font-weight:bold;font-size:12px;'>NEW BOOKING</span>
                            </div>
                        </div>
                    </div>

                    <!-- Admin Notification Body -->
                    <div style='padding:28px;'>
                        <div style='background:#fff9e6;border:1px solid #ffe8a1;border-radius:8px;padding:14px 18px;margin-bottom:24px;'>
                            <strong style='color:#8c6b00;font-size:15px;'>⚡ Action Required:</strong>
                            <p style='margin:4px 0 0 0;color:#5a4914;font-size:13px;line-height:1.5;'>
                                A customer has submitted a new booking request. Please review the details below and confirm or reject from the Admin Dashboard.
                            </p>
                        </div>

                        <!-- Customer Details Card -->
                        <div style='background:#faf7f6;border-radius:12px;border:1px solid #eedad3;padding:18px;margin-bottom:20px;'>
                            <h3 style='color:#742044;margin:0 0 12px 0;font-size:15px;border-bottom:1px solid #eedad3;padding-bottom:6px;'>
                                👤 Customer Information
                            </h3>
                            <table style='width:100%;border-collapse:collapse;font-size:13px;'>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;width:120px;'>Name:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#2c1825;'>{customerName}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Email:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#742044;'>{customerEmail}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Phone:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#2c1825;'>{customerPhone}</td>
                                </tr>
                            </table>
                        </div>

                        <!-- Appointment Details Card -->
                        <div style='background:#faf7f6;border-radius:12px;border:1px solid #eedad3;padding:18px;'>
                            <h3 style='color:#742044;margin:0 0 12px 0;font-size:15px;border-bottom:1px solid #eedad3;padding-bottom:6px;'>
                                📅 Appointment Details (ID: #{appointment.Id})
                            </h3>
                            <table style='width:100%;border-collapse:collapse;font-size:13px;'>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;width:120px;'>Service:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#2c1825;'>{serviceName} ({categoryName})</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Date:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#2c1825;'>{dateStr}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Time:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#2c1825;'>{timeStr}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Specialist:</td>
                                    <td style='padding:5px 0;font-weight:600;color:#2c1825;'>{specialist}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Amount:</td>
                                    <td style='padding:5px 0;font-weight:bold;color:#742044;font-size:15px;'>${amount}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Notes:</td>
                                    <td style='padding:5px 0;color:#55434d;'>{notes}</td>
                                </tr>
                                <tr>
                                    <td style='padding:5px 0;color:#7a6b73;'>Special Requests:</td>
                                    <td style='padding:5px 0;color:#55434d;'>{specialRequests}</td>
                                </tr>
                            </table>
                        </div>

                        <!-- Action Guidance -->
                        <div style='margin-top:24px;text-align:center;'>
                            <p style='color:#7a6b73;font-size:13px;margin:0 0 8px 0;'>
                                Once you set this appointment status to <strong>Confirmed</strong> in the Admin Dashboard, the customer will automatically receive an official confirmation email.
                            </p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div style='background:#f4f1f0;padding:16px;text-align:center;font-size:12px;color:#8f7b86;border-top:1px solid #eedad3;'>
                        &copy; {DateTime.UtcNow.Year} Luxe Glow Studio Admin Automated Notification System
                    </div>
                </div>
            </div>";
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
