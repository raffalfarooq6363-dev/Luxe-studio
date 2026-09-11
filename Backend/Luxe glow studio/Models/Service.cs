using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public int? CategoryId { get; set; }
        public virtual ServiceCategory? Category { get; set; }

        public string? ImageUrl { get; set; }

        public string? GalleryImages { get; set; } // JSON array of image URLs

        public bool IsActive { get; set; } = true;

        public bool IsPopular { get; set; } = false;

        public bool IsAvailableForHomeService { get; set; } = false;

        [Column(TypeName = "decimal(8,2)")]
        public decimal HomeServiceExtraCharge { get; set; } = 0;

        // Service details
        [StringLength(500)]
        public string? WhatToExpect { get; set; }

        [StringLength(500)]
        public string? PreparationInstructions { get; set; }

        [StringLength(500)]
        public string? AfterCareInstructions { get; set; }

        [StringLength(200)]
        public string? SuitableFor { get; set; }

        [StringLength(200)]
        public string? NotSuitableFor { get; set; }

        // Pricing options
        [Column(TypeName = "decimal(8,2)")]
        public decimal? DiscountPrice { get; set; }

        public DateTime? DiscountValidUntil { get; set; }

        // Requirements
        public int MinAdvanceBookingHours { get; set; } = 2;

        public int MaxAdvanceBookingDays { get; set; } = 30;

        public bool RequiresConsultation { get; set; } = false;

        // SEO and metadata
        [StringLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        [StringLength(160)]
        public string? MetaDescription { get; set; }

        // Statistics
        public int TotalBookings { get; set; } = 0;

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0.0m;

        public int ReviewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Appointment>? Appointments { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<PractitionerService>? PractitionerServices { get; set; }

        // Computed properties
        [NotMapped]
        public decimal EffectivePrice => DiscountPrice ?? Price;

        [NotMapped]
        public bool HasDiscount => DiscountPrice.HasValue && DiscountValidUntil > DateTime.UtcNow;
    }

    public class ServiceCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? IconClass { get; set; } // CSS class for icon

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Service>? Services { get; set; }
    }
}