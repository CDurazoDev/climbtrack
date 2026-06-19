namespace ClimbTrack.Domain.Entities;

public class SessionBlock
{
    private SessionBlock() { }

    public SessionBlock(int sessionTypeId, string name, int sortOrder)
    {
        SessionTypeId = sessionTypeId;
        Name = name;
        SortOrder = sortOrder;
    }

    public int Id { get; private set; }
    public int SessionTypeId { get; private set; }
    public string Name { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public SessionType SessionType { get; private set; } = null!;
    public ICollection<SessionBlockItem> Items { get; private set; } = [];
}

