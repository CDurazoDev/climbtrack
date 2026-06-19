using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClimbTrack.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly IPasswordResetNotificationService _passwordResetNotificationService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext db,
        IPasswordResetTokenService passwordResetTokenService,
        IPasswordResetNotificationService passwordResetNotificationService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _db = db;
        _passwordResetTokenService = passwordResetTokenService;
        _passwordResetNotificationService = passwordResetNotificationService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(user => user.Email == request.Email && user.IsActive, ct);

        if (user is null)
        {
            _logger.LogInformation("Forgot password requested for non-existing or inactive account.");
            return Result.Success();
        }

        await InvalidateOutstandingTokensAsync(user.Id, ct);

        var resetToken = _passwordResetTokenService.CreateToken();
        _db.PasswordResetTokens.Add(
            PasswordResetToken.Create(user.Id, resetToken.TokenHash, resetToken.ExpiresAt));
        await _db.SaveChangesAsync(ct);

        try
        {
            await _passwordResetNotificationService.SendResetInstructionsAsync(
                user,
                resetToken.PlainTextToken,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to send password reset instructions for user {UserId}", user.Id);
        }

        return Result.Success();
    }

    private async Task InvalidateOutstandingTokensAsync(long userId, CancellationToken ct)
    {
        var outstandingTokens = await _db.PasswordResetTokens
            .Where(token => token.UserId == userId && token.UsedAt == null && token.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var token in outstandingTokens)
        {
            token.MarkAsUsed();
        }
    }
}
