using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;
using Luxe_glow_studio.Models.DTOs;
using System.Security.Claims;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        #region Dashboard & Analytics

        /// <summary>
        /// Get comprehensive dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var dashboard = new AdminDashboardDto
            {
                // Users
                TotalUsers = await _context.Users.CountAsync(),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= thisMonth),
                ActiveCustomers = await _context.Users.CountAsync(u => u.Role == "Customer" && u.IsActive),
                ActivePractitioners = await _context.Users.CountAsync(u => u.Role == "Practitioner" && u.IsActive),

                // Appointments
                TotalAppointments = await _context.Appointments.CountAsync(),
                TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today),
                ThisWeekAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate >= thisWeek),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending"),
                ConfirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Confirmed"),

                // Revenue
                TotalRevenue = await _context.Payments.Where(p => p.Status == "Completed").SumAsync(p => p.TotalAmount),
                ThisMonthRevenue = await _context.Payments
                    .Where(p => p.Status == "Completed" && p.PaymentDate >= thisMonth)
                    .SumAsync(p => p.TotalAmount),
                LastMonthRevenue = await _context.Payments
                    .Where(p => p.Status == "Completed" && p.PaymentDate >= lastMonth && p.PaymentDate < thisMonth)
                    .SumAsync(p => p.TotalAmount),

                // Services
                TotalServices = await _context.Services.CountAsync(),
                ActiveServices = await _context.Services.CountAsync(s => s.IsActive),
                PopularServices = await _context.Services.CountAsync(s => s.IsPopular),

                // Reviews
                TotalReviews = await _context.Reviews.CountAsync(),
                AverageRating = await _context.Reviews.AverageAsync(r => (double?)r.OverallRating) ?? 0.0,
                UnreadInquiries = await _context.ContactInquiries.CountAsync(c => c.Status == "New"),

                // Recent activity
                RecentAppointments = await GetRecentAppointments(10),
                RecentPayments = await GetRecentPayments(10),
                RecentReviews = await GetRecentReviews(5)
            };

            // Calculate growth percentages
            if (dashboard.LastMonthRevenue > 0)
            {
                dashboard.RevenueGrowthPercentage = (double)((dashboard.ThisMonthRevenue - dashboard.LastMonthRevenue) / dashboard.LastMonthRevenue * 100);
            }

            return Ok(dashboard);
        }

        /// <summary>
        /// Get detailed analytics for a specific date range
        /// </summary>
        [HttpGet("analytics")]
        public async Task<ActionResult<AnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var analytics = new AnalyticsDto
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,

                // Appointments analytics
                AppointmentStats = await GetAppointmentAnalytics(startDate.Value, endDate.Value),

                // Revenue analytics
                RevenueStats = await GetRevenueAnalytics(startDate.Value, endDate.Value),

                // Service analytics
                ServiceStats = await GetServiceAnalytics(startDate.Value, endDate.Value),

                // Practitioner analytics
                PractitionerStats = await GetPractitionerAnalytics(startDate.Value, endDate.Value),

                // Customer analytics
                CustomerStats = await GetCustomerAnalytics(startDate.Value, endDate.Value),

                // Daily breakdown
                DailyBreakdown = await GetDailyAnalytics(startDate.Value, endDate.Value)
            };

            return Ok(analytics);
        }

        #endregion

        #region User Management

        /// <summary>
        /// Get all users with filtering and pagination
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserSearchDto searchDto)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var searchLower = searchDto.SearchTerm.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(searchDto.Role))
                query = query.Where(u => u.Role == searchDto.Role);

            if (searchDto.IsActive.HasValue)
                query = query.Where(u => u.IsActive == searchDto.IsActive.Value);

            if (searchDto.IsEmailVerified.HasValue)
                query = query.Where(u => u.IsEmailVerified == searchDto.IsEmailVerified.Value);

            if (searchDto.CreatedFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= searchDto.CreatedFrom.Value);

            if (searchDto.CreatedTo.HasValue)
                query = query.Where(u => u.CreatedAt <= searchDto.CreatedTo.Value);

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = searchDto.SortBy?.ToLower() switch
            {
                "name" => searchDto.SortOrder == "desc" 
                    ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                    : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
                "email" => searchDto.SortOrder == "desc" 
                    ? query.OrderByDescending(u => u.Email) 
                    : query.OrderBy(u => u.Email),
                "role" => searchDto.SortOrder == "desc" 
                    ? query.OrderByDescending(u => u.Role) 
                    : query.OrderBy(u => u.Role),
                "createdat" => searchDto.SortOrder == "desc" 
                    ? query.OrderByDescending(u => u.CreatedAt) 
                    : query.OrderBy(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            var users = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth,
                    Gender = u.Gender,
                    Address = u.Address,
                    City = u.City,
                    State = u.State,
                    PostalCode = u.PostalCode,
                    ProfileImageUrl = u.ProfileImageUrl,
                    IsActive = u.IsActive,
                    IsEmailVerified = u.IsEmailVerified,
                    IsPhoneVerified = u.IsPhoneVerified,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(new PagedResult<UserDto>
            {
                Items = users,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            });
        }

        /// <summary>
        /// Get user details by ID
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.PractitionerProfile)
                .Include(u => u.Appointments).ThenInclude(a => a.Service)
                .Include(u => u.Reviews)
                .Include(u => u.Payments)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var userDetail = new UserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                EmergencyContactRelation = user.EmergencyContactRelation,
                ReceiveEmailNotifications = user.ReceiveEmailNotifications,
                ReceiveSmsNotifications = user.ReceiveSmsNotifications,
                ReceivePromotionalEmails = user.ReceivePromotionalEmails,
                PreferredLanguage = user.PreferredLanguage,
                TotalAppointments = user.Appointments?.Count ?? 0,
                CompletedAppointments = user.Appointments?.Count(a => a.Status == "Completed") ?? 0,
                CancelledAppointments = user.Appointments?.Count(a => a.Status == "Cancelled") ?? 0,
                TotalSpent = user.Payments?.Where(p => p.Status == "Completed").Sum(p => p.TotalAmount) ?? 0,
                AverageRating = user.Reviews?.Any() == true ? user.Reviews.Average(r => r.OverallRating ?? 0) : 0,
                TotalReviews = user.Reviews?.Count ?? 0
            };

            return Ok(userDetail);
        }

        /// <summary>
        /// Update user status (active/inactive)
        /// </summary>
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto statusDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.IsActive = statusDto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the action
            await LogAction("UpdateUserStatus", "User", id, $"User status updated to {(statusDto.IsActive ? "Active" : "Inactive")}");

            return Ok(new { message = "User status updated successfully" });
        }

        #endregion

        #region Service Management

        /// <summary>
        /// Create a new service
        /// </summary>
        [HttpPost("services")]
        public async Task<ActionResult<ServiceDto>> CreateService([FromBody] CreateServiceDto serviceDto)
        {
            // Verify category exists
            var category = await _context.ServiceCategories.FindAsync(serviceDto.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Service category not found" });

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Price = serviceDto.Price,
                DurationMinutes = serviceDto.DurationMinutes,
                CategoryId = serviceDto.CategoryId,
                ImageUrl = serviceDto.ImageUrl,
                GalleryImages = serviceDto.GalleryImages != null ? string.Join(",", serviceDto.GalleryImages) : null,
                IsPopular = serviceDto.IsPopular,
                IsAvailableForHomeService = serviceDto.IsAvailableForHomeService,
                HomeServiceExtraCharge = serviceDto.HomeServiceExtraCharge,
                WhatToExpect = serviceDto.WhatToExpect,
                PreparationInstructions = serviceDto.PreparationInstructions,
                AfterCareInstructions = serviceDto.AfterCareInstructions,
                SuitableFor = serviceDto.SuitableFor,
                NotSuitableFor = serviceDto.NotSuitableFor,
                DiscountPrice = serviceDto.DiscountPrice,
                DiscountValidUntil = serviceDto.DiscountValidUntil,
                MinAdvanceBookingHours = serviceDto.MinAdvanceBookingHours,
                MaxAdvanceBookingDays = serviceDto.MaxAdvanceBookingDays,
                RequiresConsultation = serviceDto.RequiresConsultation,
                Tags = serviceDto.Tags != null ? string.Join(",", serviceDto.Tags) : null,
                MetaDescription = serviceDto.MetaDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            await LogAction("CreateService", "Service", service.Id, "New service created");

            var result = await GetServiceDto(service.Id);
            return Created($"api/services/{service.Id}", result);
        }

        // Additional methods would be implemented similarly...
        // This is a comprehensive start showing the pattern

        #endregion

        #region Helper Methods

        private async Task<List<AppointmentDto>> GetRecentAppointments(int count)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Service)
                .OrderByDescending(a => a.BookingDate)
                .Take(count)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    CustomerName = a.User!.FullName,
                    ServiceName = a.Service!.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    Status = a.Status,
                    TotalAmount = a.TotalAmount
                })
                .ToListAsync();
        }

        private async Task<List<PaymentDto>> GetRecentPayments(int count)
        {
            return await _context.Payments
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    TotalAmount = p.TotalAmount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();
        }

        private async Task<List<ReviewDto>> GetRecentReviews(int count)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Service)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    CustomerName = r.User!.FullName,
                    ServiceName = r.Service!.Name,
                    OverallRating = r.OverallRating ?? 0,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        private async Task LogAction(string action, string entityType, int? entityId, string description)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        private async Task<ServiceDto> GetServiceDto(int serviceId)
        {
            return await _context.Services
                .Include(s => s.Category)
                .Where(s => s.Id == serviceId)
                .Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    CategoryName = s.Category.Name,
                    ImageUrl = s.ImageUrl,
                    IsActive = s.IsActive,
                    IsPopular = s.IsPopular,
                    IsAvailableForHomeService = s.IsAvailableForHomeService,
                    HomeServiceExtraCharge = s.HomeServiceExtraCharge,
                    EffectivePrice = s.DiscountPrice ?? s.Price,
                    HasDiscount = s.DiscountPrice.HasValue && s.DiscountValidUntil > DateTime.UtcNow,
                    AverageRating = s.AverageRating,
                    ReviewCount = s.ReviewCount,
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync() ?? new ServiceDto();
        }

        // Additional analytics helper methods would be implemented here...
        private async Task<object> GetAppointmentAnalytics(DateTime startDate, DateTime endDate)
        {
            // Implementation for appointment analytics
            return new { };
        }

        private async Task<object> GetRevenueAnalytics(DateTime startDate, DateTime endDate)
        {
            // Implementation for revenue analytics
            return new { };
        }

        private async Task<object> GetServiceAnalytics(DateTime startDate, DateTime endDate)
        {
            // Implementation for service analytics
            return new { };
        }

        private async Task<object> GetPractitionerAnalytics(DateTime startDate, DateTime endDate)
        {
            // Implementation for practitioner analytics
            return new { };
        }

        private async Task<object> GetCustomerAnalytics(DateTime startDate, DateTime endDate)
        {
            // Implementation for customer analytics
            return new { };
        }

        private async Task<object> GetDailyAnalytics(DateTime startDate, DateTime endDate)
        {
            // Implementation for daily analytics
            return new { };
        }

        #endregion
    }

    // DTOs for Admin Controller
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveCustomers { get; set; }
        public int ActivePractitioners { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int ThisWeekAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public double RevenueGrowthPercentage { get; set; }
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int PopularServices { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int UnreadInquiries { get; set; }
        public List<AppointmentDto> RecentAppointments { get; set; } = new();
        public List<PaymentDto> RecentPayments { get; set; } = new();
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }

    public class AnalyticsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public object AppointmentStats { get; set; } = new { };
        public object RevenueStats { get; set; } = new { };
        public object ServiceStats { get; set; } = new { };
        public object PractitionerStats { get; set; } = new { };
        public object CustomerStats { get; set; } = new { };
        public object DailyBreakdown { get; set; } = new { };
    }

    public class UserSearchDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "Desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserDetailDto : UserDto
    {
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }
        public bool ReceiveEmailNotifications { get; set; }
        public bool ReceiveSmsNotifications { get; set; }
        public bool ReceivePromotionalEmails { get; set; }
        public string? PreferredLanguage { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalSpent { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
        public string? Reason { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int OverallRating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}