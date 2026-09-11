using Luxe_glow_studio.Models;

namespace Luxe_glow_studio.Services
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string storedHash);
        string GenerateJwtToken(User user);
        string GenerateRefreshToken();
        bool VerifyAdminSecretPasskey(string providedPasskey);
    }
}
