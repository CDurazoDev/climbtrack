using System.Security.Cryptography;
using System.Text;
using ClimbTrack.Application.Common.Security;
using Microsoft.Extensions.Configuration;

namespace ClimbTrack.Infrastructure.Auth;

public class PasswordResetTokenService : IPasswordResetTokenService
{
    private const int DefaultLifetimeMinutes = 30;

    private readonly IConfiguration _configuration;

    public PasswordResetTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public PasswordResetTokenData CreateToken()
    {
        var plainTextToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var tokenHash = HashToken(plainTextToken);
        var expiresAt = DateTime.UtcNow.AddMinutes(GetLifetimeMinutes());

        return new PasswordResetTokenData(plainTextToken, tokenHash, expiresAt);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private int GetLifetimeMinutes()
    {
        return int.TryParse(_configuration["Auth:PasswordResetTokenLifetimeMinutes"], out var lifetimeMinutes)
            ? lifetimeMinutes
            : DefaultLifetimeMinutes;
    }
}
