namespace ClimbTrack.Domain.Entities;

public class User
{
    private User() { }

    public User(
        string name,
        string email,
        string passwordHash,
        int difficultyLevelId,
        string role = "user",
        string preferredLocale = "es")
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        DifficultyLevelId = difficultyLevelId;
        PreferredLocale = preferredLocale;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public User(
        string email,
        string name,
        int difficultyLevelId,
        string role = "user",
        string preferredLocale = "es")
    {
        Email = email;
        Name = name;
        PasswordHash = string.Empty;
        Role = role;
        DifficultyLevelId = difficultyLevelId;
        PreferredLocale = preferredLocale;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public long Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = "user";
    public int DifficultyLevelId { get; private set; }
    public string PreferredLocale { get; private set; } = "es";
    public DifficultyLevel DifficultyLevel { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; private set; } = [];

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}

