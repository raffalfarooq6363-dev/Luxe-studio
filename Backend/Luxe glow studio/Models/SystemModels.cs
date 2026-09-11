using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    // Business Settings
    public class BusinessSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string ValueType { get; set; } = "String"; // String, Number, Boolean, JSON

        [StringLength(50)]
        public string Category { get; set; } = "General";

        public bool IsPublic { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UpdatedByUserId { get; set; }
    }

    // Business Hours
    public class BusinessHour
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string DayOfWeek { get; set; } = string.Empty;

        public bool IsOpen { get; set; } = true;

        public TimeSpan OpenTime { get; set; }

        public TimeSpan CloseTime { get; set; }

        // Break times
        public TimeSpan? BreakStartTime { get; set; }

        public TimeSpan? BreakEndTime { get; set; }

        // Special notes
        public string? Notes { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Holiday Calendar
    public class Holiday
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsRecurring { get; set; } = false;

        [StringLength(20)]
        public string? RecurrenceType { get; set; } // Yearly, Monthly

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Audit Log
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? OldValues { get; set; } // JSON

        public string? NewValues { get; set; } // JSON

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Contact/Inquiry
    public class ContactInquiry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "General"; // General, Booking, Complaint, Feedback

        [StringLength(20)]
        public string Status { get; set; } = "New"; // New, InProgress, Resolved, Closed

        public string? AdminResponse { get; set; }

        public int? AssignedToUserId { get; set; }
        public virtual User? AssignedTo { get; set; }

        public DateTime? RespondedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Loyalty Program
    public class LoyaltyPoint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int Points { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "Earned"; // Earned, Redeemed, Expired

        public int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        public int? PaymentId { get; set; }
        public virtual Payment? Payment { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Referral System
    public class Referral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReferrerUserId { get; set; }
        public virtual User ReferrerUser { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string ReferredEmail { get; set; } = string.Empty;

        public int? ReferredUserId { get; set; }
        public virtual User? ReferredUser { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferralCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Sent"; // Sent, Registered, Completed

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ReferrerReward { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ReferredReward { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // FAQ
    public class FAQ
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        public string Answer { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = "General";

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}