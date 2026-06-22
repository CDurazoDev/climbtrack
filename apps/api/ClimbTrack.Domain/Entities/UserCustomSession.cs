namespace ClimbTrack.Domain.Entities;

public class UserCustomSession
{
    private UserCustomSession() { }

    public UserCustomSession(
        long userId,
        string name,
        string colorHex,
        int loadLevel,
        string? description = null)
    {
        UserId = userId;
        Name = name;
        ColorHex = colorHex;
        LoadLevel = loadLevel;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string ColorHex { get; private set; } = null!;
    public int LoadLevel { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public ICollection<UserCustomSessionBlock> Blocks { get; private set; } = [];

    public void Update(string name, string colorHex, int loadLevel, string? description)
    {
        Name = name;
        ColorHex = colorHex;
        LoadLevel = loadLevel;
        Description = description;
    }

    public void ReplaceBlocks(IEnumerable<UserCustomSessionBlock> blocks)
    {
        Blocks.Clear();
        foreach (var block in blocks)
        {
            Blocks.Add(block);
        }
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}

