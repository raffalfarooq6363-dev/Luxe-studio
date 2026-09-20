using System.ComponentModel.DataAnnotations;

namespace Luxe_glow_studio.Models
{
    public class CreateBookingDto
    {
        public int? UserId { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public string? Notes { get; set; }

        public string? SpecialRequests { get; set; }

        public string? ClientPhone { get; set; }

        public string? ClientEmail { get; set; }

        public string? AssignedStaffMember { get; set; }

        public bool IsHomeService { get; set; } = false;

        public string? HomeServiceAddress { get; set; }

        public int? OfferCodeId { get; set; }
    }

    public class CreateMultiServiceBookingDto
    {
        public int? UserId { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public List<int> ServiceIds { get; set; } = new();

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public string? Notes { get; set; }

        public string? SpecialRequests { get; set; }

        public string? ClientPhone { get; set; }

        public string? ClientEmail { get; set; }

        public string? AssignedStaffMember { get; set; }

        public bool IsHomeService { get; set; } = false;

        public string? HomeServiceAddress { get; set; }
    }

    public class BookingAvailabilityDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int ServiceId { get; set; }

        public string? StaffMember { get; set; }
    }

    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
    }

    public class RescheduleBookingDto
    {
        [Required]
        public DateTime NewAppointmentDate { get; set; }

        [Required]
        public TimeSpan NewStartTime { get; set; }

        public string? Reason { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? Notes { get; set; }
    }

    public class BookingSearchDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int? UserId { get; set; }
        public int? ServiceId { get; set; }
        public string? StaffMember { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public int ServiceDuration { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? AssignedStaffMember { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }
        public bool IsRescheduled { get; set; }
    }

    public class BookingStatsDto
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TodayBookings { get; set; }
        public List<DailyBookingStats> WeeklyStats { get; set; } = new();
    }

    public class DailyBookingStats
    {
        public DateTime Date { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class EmailSettingsDto
    {
        public string? SmtpHost { get; set; }
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? From { get; set; }
        public string? FromName { get; set; } = "Luxe Glow Studio";
        public string? AdminEmail { get; set; }
    }
}