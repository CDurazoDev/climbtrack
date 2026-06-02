using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

        if (refreshToken is not null && !refreshToken.IsRevoked)
        {
            refreshToken.Revoke();
            await _db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
