using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClimbTrack.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenService passwordResetTokenService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _passwordResetTokenService = passwordResetTokenService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var tokenHash = _passwordResetTokenService.HashToken(request.Token);
        var resetToken = await _db.PasswordResetTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

        if (resetToken is null)
        {
            return Result.Failure("Invalid password reset token.");
        }

        if (resetToken.IsUsed)
        {
            return Result.Failure("This password reset token has already been used.");
        }

        if (resetToken.IsExpired)
        {
            return Result.Failure("This password reset token has expired.");
        }

        if (!resetToken.User.IsActive)
        {
            _logger.LogWarning("Password reset rejected because user {UserId} is inactive.", resetToken.UserId);
            return Result.Failure("The account is inactive.");
        }

        resetToken.User.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        resetToken.MarkAsUsed();

        var activeRefreshTokens = await _db.RefreshTokens
            .Where(token => token.UserId == resetToken.UserId && !token.IsRevoked)
            .ToListAsync(ct);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke();
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
