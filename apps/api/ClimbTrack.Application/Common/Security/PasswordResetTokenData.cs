namespace ClimbTrack.Application.Common.Security;

public record PasswordResetTokenData(
    string PlainTextToken,
    string TokenHash,
    DateTime ExpiresAt);
