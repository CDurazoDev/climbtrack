namespace ClimbTrack.Domain.Entities;

public class RefreshToken
{
    private RefreshToken() { }

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public string FamilyId { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public User User { get; private set; } = null!;

    public static RefreshToken Create(long userId, string tokenHash, string familyId)
        => new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
        };

    public void Revoke() => IsRevoked = true;
}
