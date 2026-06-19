namespace ClimbTrack.Domain.Entities;

public class PasswordResetToken
{
    private PasswordResetToken() { }

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public User User { get; private set; } = null!;

    public bool IsUsed => UsedAt.HasValue;
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    public static PasswordResetToken Create(long userId, string tokenHash, DateTime expiresAt)
        => new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
        };

    public void MarkAsUsed()
    {
        UsedAt ??= DateTime.UtcNow;
    }
}
