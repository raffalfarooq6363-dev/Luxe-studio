using System.ComponentModel.DataAnnotations;

namespace Luxe_glow_studio.Models.DTOs
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<string>? GalleryImages { get; set; }
        public bool IsActive { get; set; }
        public bool IsPopular { get; set; }
        public bool IsAvailableForHomeService { get; set; }
        public decimal HomeServiceExtraCharge { get; set; }
        public string? WhatToExpect { get; set; }
        public string? PreparationInstructions { get; set; }
        public string? AfterCareInstructions { get; set; }
        public string? SuitableFor { get; set; }
        public string? NotSuitableFor { get; set; }
        public decimal? DiscountPrice { get; set; }
        public DateTime? DiscountValidUntil { get; set; }
        public decimal EffectivePrice { get; set; }
        public bool HasDiscount { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PractitionerDto>? AvailablePractitioners { get; set; }
    }

    public class CreateServiceDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        [Range(15, 480)]
        public int DurationMinutes { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        public List<string>? GalleryImages { get; set; }

        public bool IsPopular { get; set; } = false;

        public bool IsAvailableForHomeService { get; set; } = false;

        [Range(0, 999.99)]
        public decimal HomeServiceExtraCharge { get; set; } = 0;

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

        [Range(0.01, 999999.99)]
        public decimal? DiscountPrice { get; set; }

        public DateTime? DiscountValidUntil { get; set; }

        [Range(2, 720)]
        public int MinAdvanceBookingHours { get; set; } = 2;

        [Range(1, 365)]
        public int MaxAdvanceBookingDays { get; set; } = 30;

        public bool RequiresConsultation { get; set; } = false;

        public List<string>? Tags { get; set; }

        [StringLength(160)]
        public string? MetaDescription { get; set; }
    }

    public class UpdateServiceDto : CreateServiceDto
    {
        public bool IsActive { get; set; } = true;
    }

    public class ServiceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int ServiceCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? IconClass { get; set; }

        public string? ImageUrl { get; set; }

        public int SortOrder { get; set; } = 0;
    }

    public class ServiceSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public bool? IsAvailableForHomeService { get; set; }
        public bool? IsPopular { get; set; }
        public bool? HasDiscount { get; set; }
        public List<string>? Tags { get; set; }
        public string? SortBy { get; set; } = "Name"; // Name, Price, Duration, Rating, Popular
        public string? SortOrder { get; set; } = "Asc"; // Asc, Desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ServiceStatsDto
    {
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int PopularServices { get; set; }
        public int HomeServiceAvailable { get; set; }
        public int ServicesWithDiscount { get; set; }
        public decimal AverageServicePrice { get; set; }
        public decimal AverageServiceDuration { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalBookings { get; set; }
        public List<CategoryStatsDto> CategoryStats { get; set; } = new();
    }

    public class CategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class PractitionerDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public int YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public List<string>? Specializations { get; set; }
        public decimal Rating { get; set; }
        public int TotalReviews { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsAvailableForHomeService { get; set; }
        public decimal HomeServiceExtraCharge { get; set; }
        public bool IsActive { get; set; }
        public List<string>? PortfolioImages { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
}