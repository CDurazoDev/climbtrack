namespace ClimbTrack.Domain.Entities;

public class SessionType
{
    private SessionType() { }

    public SessionType(
        string code,
        string name,
        string colorHex,
        int loadLevel,
        int energySystemId,
        string? description = null)
    {
        Code = code;
        Name = name;
        ColorHex = colorHex;
        LoadLevel = loadLevel;
        EnergySystemId = energySystemId;
        Description = description;
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string ColorHex { get; private set; } = null!;
    public int LoadLevel { get; private set; }
    public string? Description { get; private set; }
    public int EnergySystemId { get; private set; }
    public EnergySystem EnergySystem { get; private set; } = null!;
    public ICollection<SessionBlock> Blocks { get; private set; } = [];
}

