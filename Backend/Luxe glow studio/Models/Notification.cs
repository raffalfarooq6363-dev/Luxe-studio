using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Booking, Payment, Promotion, System, Reminder

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        // Related entities
        public int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        public int? PaymentId { get; set; }
        public virtual Payment? Payment { get; set; }

        public int? PromotionId { get; set; }
        public virtual Promotion? Promotion { get; set; }

        // Delivery channels
        public bool SendEmail { get; set; } = true;

        public bool SendSms { get; set; } = false;

        public bool SendPush { get; set; } = true;

        public bool SendInApp { get; set; } = true;

        // Delivery status
        public bool EmailSent { get; set; } = false;

        public bool SmsSent { get; set; } = false;

        public bool PushSent { get; set; } = false;

        public DateTime? EmailSentAt { get; set; }

        public DateTime? SmsSentAt { get; set; }

        public DateTime? PushSentAt { get; set; }

        // Delivery failures
        public string? EmailError { get; set; }

        public string? SmsError { get; set; }

        public string? PushError { get; set; }

        // Scheduling
        public DateTime? ScheduledFor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Action data (JSON)
        public string? ActionData { get; set; }

        public string? ActionUrl { get; set; }

        // Template information
        public string? TemplateId { get; set; }

        public string? TemplateData { get; set; } // JSON data for template

        // Computed properties
        [NotMapped]
        public bool IsDelivered => (SendEmail ? EmailSent : true) && 
                                  (SendSms ? SmsSent : true) && 
                                  (SendPush ? PushSent : true);

        [NotMapped]
        public bool HasErrors => !string.IsNullOrEmpty(EmailError) || 
                                !string.IsNullOrEmpty(SmsError) || 
                                !string.IsNullOrEmpty(PushError);
    }

    // Notification templates
    public class NotificationTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Email, SMS, Push, InApp

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        // Template variables (JSON array of variable names)
        public string? Variables { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string Language { get; set; } = "English";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // User notification preferences
    public class NotificationPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        public bool EmailEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = false;

        public bool PushEnabled { get; set; } = true;

        public bool InAppEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}