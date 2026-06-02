using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Entities;

namespace ClimbTrack.Application.Common.Security;

public interface ITokenService
{
    Task<AuthTokensDto> GenerateTokensAsync(User user, CancellationToken cancellationToken, string? familyId = null);
    string HashRefreshToken(string refreshToken);
}

