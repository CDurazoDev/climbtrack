using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClimbTrack.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private const int AccessTokenLifetimeMinutes = 15;

    private readonly IConfiguration _config;
    private readonly ClimbTrackDbContext _db;

    public TokenService(IConfiguration config, ClimbTrackDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<AuthTokensDto> GenerateTokensAsync(
        User user,
        CancellationToken cancellationToken,
        string? familyId = null)
    {
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenLifetimeMinutes);
        var accessToken = GenerateJwt(user, accessTokenExpiresAt);
        var rawToken = GenerateSecureToken();
        var tokenHash = HashRefreshToken(rawToken);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, familyId ?? Guid.NewGuid().ToString());

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        await EnsureDifficultyLevelLoadedAsync(user, cancellationToken);

        return new AuthTokensDto(
            accessToken,
            rawToken,
            accessTokenExpiresAt,
            MapToProfileDto(user));
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private string GenerateJwt(User user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetRequiredJwtSecret()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetRequiredJwtSecret()
    {
        var secret = _config["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret) || secret == "CHANGE_ME")
        {
            throw new InvalidOperationException("Jwt:Secret must be configured from a secure local source.");
        }

        return secret;
    }

    private async Task EnsureDifficultyLevelLoadedAsync(User user, CancellationToken cancellationToken)
    {
        if (user.DifficultyLevel is not null)
        {
            return;
        }

        await _db.Entry(user)
            .Reference(x => x.DifficultyLevel)
            .LoadAsync(cancellationToken);
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.DifficultyLevel.Code,
            user.PreferredLocale);
    }
}
