using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Customer"; // Admin, Customer, Practitioner

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; } // Male, Female, Other

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; } = "India";

        public string? ProfileImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEmailVerified { get; set; } = false;

        public bool IsPhoneVerified { get; set; } = false;

        public string? EmailVerificationToken { get; set; }

        public string? PhoneVerificationCode { get; set; }

        public DateTime? EmailVerificationExpires { get; set; }

        public DateTime? PhoneVerificationExpires { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetExpires { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Emergency contact information
        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactRelation { get; set; }

        // Preferences
        public bool ReceiveEmailNotifications { get; set; } = true;

        public bool ReceiveSmsNotifications { get; set; } = true;

        public bool ReceivePromotionalEmails { get; set; } = true;

        [StringLength(10)]
        public string? PreferredLanguage { get; set; } = "English";

        // Navigation properties
        public virtual ICollection<Appointment>? Appointments { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
        public virtual Practitioner? PractitionerProfile { get; set; }

        // Computed property for full name
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Computed property for display name
        [NotMapped]
        public string DisplayName => string.IsNullOrWhiteSpace(FirstName) ? Email : FullName;
    }
}