namespace ClimbTrack.Domain.Entities;

public class SessionBlockItem
{
    private SessionBlockItem() { }

    public SessionBlockItem(int sessionBlockId, string description, int sortOrder)
    {
        SessionBlockId = sessionBlockId;
        Description = description;
        SortOrder = sortOrder;
    }

    public int Id { get; private set; }
    public int SessionBlockId { get; private set; }
    public string Description { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public SessionBlock Block { get; private set; } = null!;
}
