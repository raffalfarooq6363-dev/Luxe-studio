using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, InProgress, Completed, Cancelled, NoShow

        public string? Notes { get; set; }

        public string? SpecialRequests { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancellationReason { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public bool IsRescheduled { get; set; } = false;

        public int? RescheduledFromId { get; set; }

        public bool SendReminder { get; set; } = true;

        public DateTime? ReminderSentAt { get; set; }

        // Staff member assigned to this appointment
        public string? AssignedStaffMember { get; set; }

        // Client contact information
        public string? ClientPhone { get; set; }

        public string? ClientEmail { get; set; }
    }
}