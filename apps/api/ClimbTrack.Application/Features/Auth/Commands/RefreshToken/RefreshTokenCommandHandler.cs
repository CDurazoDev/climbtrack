using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Application.Features.Auth.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens
            .Include(token => token.User)
            .ThenInclude(user => user.DifficultyLevel)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

        if (refreshToken is null)
        {
            return Result.Failure<AuthTokensDto>("Invalid refresh token.");
        }

        if (refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTime.UtcNow || !refreshToken.User.IsActive)
        {
            await RevokeFamilyAsync(refreshToken.FamilyId, ct);
            return Result.Failure<AuthTokensDto>("Invalid refresh token.");
        }

        refreshToken.Revoke();
        await _db.SaveChangesAsync(ct);

        return Result.Success(await _tokenService.GenerateTokensAsync(refreshToken.User, ct, refreshToken.FamilyId));
    }

    private async Task RevokeFamilyAsync(string familyId, CancellationToken ct)
    {
        var familyTokens = await _db.RefreshTokens
            .Where(token => token.FamilyId == familyId && !token.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in familyTokens)
        {
            token.Revoke();
        }

        await _db.SaveChangesAsync(ct);
    }
}

