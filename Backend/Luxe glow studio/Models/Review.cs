using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; } = null!;

        public int? PractitionerId { get; set; }
        public virtual Practitioner? Practitioner { get; set; }

        public int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        [Required]
        [Range(1, 5)]
        public int ServiceRating { get; set; }

        [Range(1, 5)]
        public int? PractitionerRating { get; set; }

        [Range(1, 5)]
        public int? OverallRating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        // Specific rating categories
        [Range(1, 5)]
        public int? QualityRating { get; set; }

        [Range(1, 5)]
        public int? TimelinessRating { get; set; }

        [Range(1, 5)]
        public int? CleanlinessRating { get; set; }

        [Range(1, 5)]
        public int? ValueForMoneyRating { get; set; }

        // Review metadata
        public bool IsVerified { get; set; } = true; // Only customers who booked can review

        public bool IsPublic { get; set; } = true;

        public bool IsRecommended { get; set; } = false;

        // Admin moderation
        public bool IsApproved { get; set; } = true;

        public string? AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Review images
        public string? ReviewImages { get; set; } // JSON array of image URLs

        // Helpful votes
        public int HelpfulVotes { get; set; } = 0;

        public int UnhelpfulVotes { get; set; } = 0;

        // Response from business
        public string? BusinessResponse { get; set; }

        public DateTime? BusinessResponseDate { get; set; }

        public int? BusinessRespondentId { get; set; }

        // Navigation properties
        public virtual ICollection<ReviewVote>? Votes { get; set; }
    }

    public class ReviewVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }
        public virtual Review Review { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public bool IsHelpful { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}