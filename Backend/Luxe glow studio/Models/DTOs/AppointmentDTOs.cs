using System.ComponentModel.DataAnnotations;

namespace Luxe_glow_studio.Models.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public int ServiceId { get; set; }

        public int? PractitionerId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }

        public bool IsHomeService { get; set; } = false;

        [StringLength(500)]
        public string? HomeServiceAddress { get; set; }

        [StringLength(20)]
        public string? ClientPhone { get; set; }

        [StringLength(255)]
        public string? ClientEmail { get; set; }

        public string? PreferredPaymentMethod { get; set; }

        public bool SendReminder { get; set; } = true;
    }

    public class AppointmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public int ServiceDuration { get; set; }
        public int? PractitionerId { get; set; }
        public string? PractitionerName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SpecialRequests { get; set; }
        public bool IsHomeService { get; set; }
        public string? HomeServiceAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public bool IsRescheduled { get; set; }
        public string? AssignedStaffMember { get; set; }
        public PaymentDto? Payment { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReschedule { get; set; }
        public bool CanReview { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public int? PractitionerId { get; set; }
    }

    public class RescheduleAppointmentDto
    {
        [Required]
        public DateTime NewAppointmentDate { get; set; }

        [Required]
        public TimeSpan NewStartTime { get; set; }

        public string? Reason { get; set; }

        public int? NewPractitionerId { get; set; }
    }

    public class AppointmentSearchDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int? UserId { get; set; }
        public int? ServiceId { get; set; }
        public int? PractitionerId { get; set; }
        public bool? IsHomeService { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "AppointmentDate";
        public string? SortOrder { get; set; } = "Desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AvailabilityCheckDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int ServiceId { get; set; }

        public int? PractitionerId { get; set; }

        public bool IsHomeService { get; set; } = false;
    }

    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
        public List<int>? AvailablePractitionerIds { get; set; }
        public bool IsPopular { get; set; } = false;
        public decimal? PriceModifier { get; set; }
    }

    public class AppointmentStatsDto
    {
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NoShowAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int ThisWeekAppointments { get; set; }
        public int ThisMonthAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public double AverageRating { get; set; }
        public int HomeServiceAppointments { get; set; }
        public int StudioAppointments { get; set; }
        public List<DailyAppointmentStats> DailyStats { get; set; } = new();
        public List<ServiceAppointmentStats> ServiceStats { get; set; } = new();
        public List<PractitionerAppointmentStats> PractitionerStats { get; set; } = new();
    }

    public class DailyAppointmentStats
    {
        public DateTime Date { get; set; }
        public int AppointmentCount { get; set; }
        public decimal Revenue { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class ServiceAppointmentStats
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class PractitionerAppointmentStats
    {
        public int PractitionerId { get; set; }
        public string PractitionerName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal CommissionEarned { get; set; }
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public bool IsRefunded { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? CouponCode { get; set; }
        public string? ReceiptUrl { get; set; }
    }
}