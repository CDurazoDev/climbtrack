namespace ClimbTrack.Domain.Entities;

public class Phase
{
    private Phase() { }

    public Phase(string code, string name, int sortOrder, string? description = null)
    {
        Code = code;
        Name = name;
        SortOrder = sortOrder;
        Description = description;
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
}

