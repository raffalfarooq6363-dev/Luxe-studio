using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class Practitioner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        public int YearsOfExperience { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [StringLength(500)]
        public string? Specializations { get; set; } // JSON array of specialization IDs

        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 0.0m;

        public int TotalReviews { get; set; } = 0;

        [Column(TypeName = "decimal(8,2)")]
        public decimal HourlyRate { get; set; }

        public bool IsAvailableForHomeService { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        public decimal HomeServiceExtraCharge { get; set; } = 0;

        [Column(TypeName = "decimal(8,2)")]
        public decimal CommissionRate { get; set; } = 0.30m; // 30% default

        public bool IsActive { get; set; } = true;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Working hours (JSON format)
        public string? WorkingHours { get; set; }

        // Break times (JSON format)
        public string? BreakTimes { get; set; }

        // Days off (JSON array of day names)
        public string? DaysOff { get; set; }

        // Certifications and qualifications
        public string? Certifications { get; set; }

        // Social media links
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? LinkedInUrl { get; set; }

        // Professional photos
        public string? PortfolioImages { get; set; } // JSON array of image URLs

        // Navigation properties
        public virtual ICollection<Appointment>? Appointments { get; set; }
        public virtual ICollection<Review>? PractitionerReviews { get; set; }
        public virtual ICollection<PractitionerService>? PractitionerServices { get; set; }
        public virtual ICollection<PractitionerAvailability>? Availability { get; set; }
    }

    public class PractitionerService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PractitiOnerId { get; set; }
        public virtual Practitioner Practitioner { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; } = null!;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CustomPrice { get; set; }

        public int? CustomDuration { get; set; } // In minutes

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PractitionerAvailability
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PractitiOnerId { get; set; }
        public virtual Practitioner Practitioner { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday, etc.

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Break times
        public TimeSpan? BreakStartTime { get; set; }
        public TimeSpan? BreakEndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}