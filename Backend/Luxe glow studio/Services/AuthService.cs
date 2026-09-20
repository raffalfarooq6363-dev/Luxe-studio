using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Luxe_glow_studio.Models;

namespace Luxe_glow_studio.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            // Check if format is salt:hash
            var parts = storedHash.Split(':');
            if (parts.Length == 2)
            {
                try
                {
                    byte[] salt = Convert.FromBase64String(parts[0]);
                    byte[] expectedHash = Convert.FromBase64String(parts[1]);

                    byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                        Encoding.UTF8.GetBytes(password),
                        salt,
                        Iterations,
                        HashAlgorithmName.SHA256,
                        HashSize);

                    return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
                }
                catch
                {
                    // Fall back to plain string comparison if decoding fails
                }
            }

            // Fallback for legacy / plain-text passwords
            return password == storedHash;
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("Jwt:Key must be configured outside source control.");
            var issuer = _configuration["Jwt:Issuer"] ?? "LuxeGlowStudio";
            var audience = _configuration["Jwt:Audience"] ?? "LuxeGlowStudioUsers";
            var expireDays = int.TryParse(_configuration["Jwt:ExpireDays"], out var days) ? days : 7;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("role", user.Role),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public bool VerifyAdminSecretPasskey(string providedPasskey)
        {
            var configuredSecret = _configuration["AdminSecurity:SecretPasskey"];
            if (string.IsNullOrWhiteSpace(configuredSecret))
                return false;
            return string.Equals(providedPasskey?.Trim(), configuredSecret.Trim(), StringComparison.Ordinal);
        }
    }
}
