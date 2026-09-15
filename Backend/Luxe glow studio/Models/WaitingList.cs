using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class WaitingList
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
        public DateTime PreferredDate { get; set; }

        [StringLength(100)]
        public string? PreferredTimeRange { get; set; } // e.g., "Morning", "Afternoon", "9AM-12PM"

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Notified, Booked, Expired

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? NotifiedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int? ResultingAppointmentId { get; set; }
        [ForeignKey("ResultingAppointmentId")]
        public Appointment? ResultingAppointment { get; set; }
    }

    public class WaitingListDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime PreferredDate { get; set; }

        public string? PreferredTimeRange { get; set; }

        public string? Notes { get; set; }
    }
}
