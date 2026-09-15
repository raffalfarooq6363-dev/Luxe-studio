using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;
using Microsoft.EntityFrameworkCore;

namespace Luxe_glow_studio.Services
{
    public interface INotificationService
    {
        Task SendBookingReminderAsync(int appointmentId);
        Task SendBulkRemindersAsync();
        Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendBookingReminderAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || !appointment.SendReminder)
            {
                return;
            }

            // Check if reminder already sent
            if (appointment.ReminderSentAt.HasValue)
            {
                return;
            }

            var notification = new Notification
            {
                UserId = appointment.UserId,
                Type = "Reminder",
                Title = "Upcoming Appointment Reminder",
                Message = $"Reminder: You have an appointment for {appointment.Service?.Name} tomorrow at {appointment.StartTime}. " +
                         $"Location: Luxe Glow Studio. If you need to reschedule, please contact us.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            appointment.ReminderSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reminder sent for appointment {appointmentId}");

            // Here you would integrate with email/SMS service
            await SendEmailReminderAsync(appointment);
            await SendSMSReminderAsync(appointment);
        }

        public async Task SendBulkRemindersAsync()
        {
            // Get appointments for tomorrow that haven't been reminded
            var tomorrow = DateTime.Today.AddDays(1);
            
            var appointmentsToRemind = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate.Date == tomorrow &&
                           a.SendReminder &&
                           !a.ReminderSentAt.HasValue &&
                           a.Status != "Cancelled" &&
                           a.Status != "Completed")
                .ToListAsync();

            _logger.LogInformation($"Sending reminders for {appointmentsToRemind.Count} appointments");

            foreach (var appointment in appointmentsToRemind)
            {
                try
                {
                    await SendBookingReminderAsync(appointment.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send reminder for appointment {appointment.Id}");
                }
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            var query = _context.Notifications.Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task SendEmailReminderAsync(Appointment appointment)
        {
            // Integrate with your email service (SendGrid, AWS SES, etc.)
            _logger.LogInformation($"Sending email reminder to {appointment.ClientEmail}");
            
            // Placeholder for actual email implementation
            await Task.CompletedTask;
        }

        private async Task SendSMSReminderAsync(Appointment appointment)
        {
            // Integrate with your SMS service (Twilio, AWS SNS, etc.)
            _logger.LogInformation($"Sending SMS reminder to {appointment.ClientPhone}");
            
            // Placeholder for actual SMS implementation
            await Task.CompletedTask;
        }
    }

    public class BookingReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingReminderBackgroundService> _logger;

        public BookingReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Reminder Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run at 6 PM every day
                    var now = DateTime.Now;
                    var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0);

                    if (now > scheduledTime)
                    {
                        scheduledTime = scheduledTime.AddDays(1);
                    }

                    var delay = scheduledTime - now;
                    _logger.LogInformation($"Next reminder check scheduled at {scheduledTime}");

                    await Task.Delay(delay, stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notificationService.SendBulkRemindersAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Booking Reminder Background Service");
                }

                // Wait 1 hour before checking again
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
