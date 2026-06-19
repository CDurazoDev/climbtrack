namespace ClimbTrack.Domain.Entities;

public class TrainingType
{
    private TrainingType() { }

    public TrainingType(string code, string name, string? description = null)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
}
