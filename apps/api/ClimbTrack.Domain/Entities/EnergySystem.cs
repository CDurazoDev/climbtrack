namespace ClimbTrack.Domain.Entities;

public class EnergySystem
{
    private EnergySystem() { }

    public EnergySystem(string code, string name, string? durationRange = null)
    {
        Code = code;
        Name = name;
        DurationRange = durationRange;
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? DurationRange { get; private set; }
}

