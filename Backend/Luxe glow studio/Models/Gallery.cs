using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luxe_glow_studio.Models
{
    public class GalleryImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Before/After, Services, Interior, Team, Events

        // Related entities
        public int? ServiceId { get; set; }
        public virtual Service? Service { get; set; }

        public int? PractitionerId { get; set; }
        public virtual Practitioner? Practitioner { get; set; }

        // Image metadata
        public string? AltText { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        // Before/After specific fields
        public bool IsBeforeAfter { get; set; } = false;

        public string? BeforeImageUrl { get; set; }

        public string? AfterImageUrl { get; set; }

        [StringLength(200)]
        public string? TreatmentDetails { get; set; }

        // Engagement metrics
        public int ViewCount { get; set; } = 0;

        public int LikeCount { get; set; } = 0;

        public int ShareCount { get; set; } = 0;

        // Upload information
        public int UploadedByUserId { get; set; }

        public string? OriginalFileName { get; set; }

        public long? FileSizeBytes { get; set; }

        [StringLength(50)]
        public string? MimeType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<GalleryImageLike>? Likes { get; set; }

        // Computed properties
        [NotMapped]
        public string DisplayImageUrl => !string.IsNullOrEmpty(ThumbnailUrl) ? ThumbnailUrl : ImageUrl;

        [NotMapped]
        public double FileSizeMB => FileSizeBytes.HasValue ? Math.Round((double)FileSizeBytes.Value / (1024 * 1024), 2) : 0;
    }

    public class GalleryImageLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GalleryImageId { get; set; }
        public virtual GalleryImage GalleryImage { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Portfolio for practitioners
    public class Portfolio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PractitionerId { get; set; }
        public virtual Practitioner Practitioner { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        // Service information
        public int? ServiceId { get; set; }
        public virtual Service? Service { get; set; }

        [StringLength(200)]
        public string? ServiceName { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ServicePrice { get; set; }

        public int? ServiceDuration { get; set; }

        // Client information (anonymized)
        [StringLength(50)]
        public string? ClientAge { get; set; }

        [StringLength(50)]
        public string? ClientSkinType { get; set; }

        [StringLength(200)]
        public string? ClientConcerns { get; set; }

        // Treatment details
        [StringLength(500)]
        public string? TreatmentProcess { get; set; }

        [StringLength(500)]
        public string? ProductsUsed { get; set; }

        [StringLength(500)]
        public string? Results { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}