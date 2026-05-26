namespace ClimbTrack.Domain.Entities;

public class User
{
    private User() { }

    public User(
        string email,
        string name,
        int difficultyLevelId,
        string role = "user",
        string preferredLocale = "es")
    {
        Email = email;
        Name = name;
        Role = role;
        DifficultyLevelId = difficultyLevelId;
        PreferredLocale = preferredLocale;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public long Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Role { get; private set; } = "user";
    public int DifficultyLevelId { get; private set; }
    public string PreferredLocale { get; private set; } = "es";
    public DifficultyLevel DifficultyLevel { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
}

