using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Luxe_glow_studio.Data;
using Luxe_glow_studio.Models;
using Luxe_glow_studio.Models.DTOs;
using Luxe_glow_studio.Services;

namespace Luxe_glow_studio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AuthController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Register a new customer account
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid registration data", errors = ModelState });
            }

            var normalizedEmail = registerDto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return BadRequest(new { message = "An account with this email already exists" });
            }

            var user = new User
            {
                FirstName = registerDto.FirstName.Trim(),
                LastName = registerDto.LastName.Trim(),
                Email = normalizedEmail,
                PasswordHash = _authService.HashPassword(registerDto.Password),
                Role = "Customer",
                PhoneNumber = registerDto.PhoneNumber?.Trim(),
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender?.Trim(),
                Address = registerDto.Address?.Trim(),
                City = registerDto.City?.Trim(),
                State = registerDto.State?.Trim(),
                PostalCode = registerDto.PostalCode?.Trim(),
                ReceiveEmailNotifications = registerDto.ReceiveEmailNotifications,
                ReceiveSmsNotifications = registerDto.ReceiveSmsNotifications,
                ReceivePromotionalEmails = registerDto.ReceivePromotionalEmails,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            var userDto = MapToUserDto(user);

            return CreatedAtAction(nameof(GetCurrentUser), new { id = user.Id }, new LoginResponseDto
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = userDto,
                RefreshToken = refreshToken
            });
        }

        /// <summary>
        /// Customer login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid login data" });
            }

            var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users
                .Include(u => u.PractitionerProfile)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Your account has been deactivated. Please contact support." });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            var userDto = MapToUserDto(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = userDto,
                RefreshToken = refreshToken
            });
        }

        /// <summary>
        /// Admin login with secret passkey
        /// </summary>
        [HttpPost("admin/login")]
        public async Task<ActionResult<LoginResponseDto>> AdminLogin([FromBody] AdminLoginDto adminLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid login parameters" });
            }

            var normalizedEmail = adminLoginDto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users
                .Include(u => u.PractitionerProfile)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !_authService.VerifyPassword(adminLoginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid admin credentials" });
            }

            if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access Denied: Administrator privileges required" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Admin account has been deactivated" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            var userDto = MapToUserDto(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = userDto,
                RefreshToken = refreshToken
            });
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token claims" });
            }

            var user = await _context.Users
                .Include(u => u.PractitionerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = MapToUserDto(user);
            return Ok(userDto);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto profileDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token claims" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user properties
            user.FirstName = profileDto.FirstName.Trim();
            user.LastName = profileDto.LastName.Trim();
            user.PhoneNumber = profileDto.PhoneNumber?.Trim();
            user.DateOfBirth = profileDto.DateOfBirth;
            user.Gender = profileDto.Gender?.Trim();
            user.Address = profileDto.Address?.Trim();
            user.City = profileDto.City?.Trim();
            user.State = profileDto.State?.Trim();
            user.PostalCode = profileDto.PostalCode?.Trim();
            user.ProfileImageUrl = profileDto.ProfileImageUrl?.Trim();
            user.EmergencyContactName = profileDto.EmergencyContactName?.Trim();
            user.EmergencyContactPhone = profileDto.EmergencyContactPhone?.Trim();
            user.EmergencyContactRelation = profileDto.EmergencyContactRelation?.Trim();
            user.ReceiveEmailNotifications = profileDto.ReceiveEmailNotifications;
            user.ReceiveSmsNotifications = profileDto.ReceiveSmsNotifications;
            user.ReceivePromotionalEmails = profileDto.ReceivePromotionalEmails;
            user.PreferredLanguage = profileDto.PreferredLanguage?.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userDto = MapToUserDto(user);
            return Ok(userDto);
        }

        /// <summary>
        /// Change password
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid token claims" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (!_authService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            user.PasswordHash = _authService.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Admin setup endpoint for initial admin account creation
        /// </summary>
        [HttpPost("admin/setup")]
        public async Task<IActionResult> SetupAdmin([FromBody] AdminSetupDto setupDto)
        {
            var normalizedEmail = setupDto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return BadRequest(new { message = "An account with this email already exists" });
            }

            var admin = new User
            {
                FirstName = setupDto.FirstName.Trim(),
                LastName = setupDto.LastName.Trim(),
                Email = normalizedEmail,
                PasswordHash = _authService.HashPassword(setupDto.Password),
                Role = "Admin",
                PhoneNumber = setupDto.PhoneNumber?.Trim(),
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin account created successfully" });
        }

        #region Helper Methods

        private UserDto MapToUserDto(User user)
        {
            var userDto = new UserDto
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
                LastLoginAt = user.LastLoginAt
            };

            if (user.PractitionerProfile != null)
            {
                userDto.PractitionerProfile = new PractitionerDto
                {
                    Id = user.PractitionerProfile.Id,
                    UserId = user.PractitionerProfile.UserId,
                    FullName = user.FullName,
                    ProfileImageUrl = user.ProfileImageUrl,
                    YearsOfExperience = user.PractitionerProfile.YearsOfExperience,
                    Bio = user.PractitionerProfile.Bio,
                    Rating = user.PractitionerProfile.Rating,
                    TotalReviews = user.PractitionerProfile.TotalReviews,
                    HourlyRate = user.PractitionerProfile.HourlyRate,
                    IsAvailableForHomeService = user.PractitionerProfile.IsAvailableForHomeService,
                    HomeServiceExtraCharge = user.PractitionerProfile.HomeServiceExtraCharge,
                    IsActive = user.PractitionerProfile.IsActive,
                    InstagramUrl = user.PractitionerProfile.InstagramUrl,
                    FacebookUrl = user.PractitionerProfile.FacebookUrl
                };
            }

            return userDto;
        }

        #endregion
    }

    // Additional DTOs for Auth Controller
    public class AdminLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }

    public class AdminSetupDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

    }
}
