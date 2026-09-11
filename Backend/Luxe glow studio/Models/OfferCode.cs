using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class OfferCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty; // Percentage, Fixed, FreeService

        [Column(TypeName = "decimal(8,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinimumOrderAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaximumDiscountAmount { get; set; }

        public int? MaxUsageCount { get; set; }

        public int CurrentUsageCount { get; set; } = 0;

        public int? MaxUsagePerCustomer { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidUntil { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPublic { get; set; } = true; // If false, only specific customers can use

        // Applicable services (JSON array of service IDs)
        public string? ApplicableServices { get; set; }

        // Applicable categories (JSON array of category IDs)
        public string? ApplicableCategories { get; set; }

        // Applicable customer segments
        [StringLength(500)]
        public string? CustomerSegments { get; set; } // NewCustomer, Returning, VIP, etc.

        // Day/Time restrictions
        public string? ApplicableDays { get; set; } // JSON array of day names

        public TimeSpan? ApplicableTimeStart { get; set; }

        public TimeSpan? ApplicableTimeEnd { get; set; }

        // Special conditions
        public bool IsFirstBookingOnly { get; set; } = false;

        public bool IsBirthdayOffer { get; set; } = false;

        public int? BirthdayOfferDays { get; set; } // Days before/after birthday

        // Creator information
        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Usage tracking
        public virtual ICollection<OfferCodeUsage>? Usages { get; set; }

        public virtual ICollection<Payment>? Payments { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsValid => IsActive && DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidUntil;

        [NotMapped]
        public bool HasUsageLeft => !MaxUsageCount.HasValue || CurrentUsageCount < MaxUsageCount.Value;

        [NotMapped]
        public int RemainingUsage => MaxUsageCount.HasValue ? Math.Max(0, MaxUsageCount.Value - CurrentUsageCount) : int.MaxValue;
    }

    public class OfferCodeUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OfferCodeId { get; set; }
        public virtual OfferCode OfferCode { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int PaymentId { get; set; }
        public virtual Payment Payment { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal OrderAmount { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }

    // Promotion campaigns
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string PromotionType { get; set; } = string.Empty; // Discount, BOGO, Package, Seasonal

        public string? ImageUrl { get; set; }

        public string? BannerImageUrl { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        // Terms and conditions
        [StringLength(2000)]
        public string? TermsAndConditions { get; set; }

        // Related offer codes
        public string? OfferCodes { get; set; } // JSON array of offer code IDs

        // Targeting
        public string? TargetAudience { get; set; } // JSON array of customer segments

        public int ViewCount { get; set; } = 0;

        public int ClickCount { get; set; } = 0;

        public int ConversionCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Computed properties
        [NotMapped]
        public bool IsCurrentlyActive => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

        [NotMapped]
        public double ConversionRate => ClickCount > 0 ? (double)ConversionCount / ClickCount * 100 : 0;
    }
}