namespace ClimbTrack.Domain.Entities;

public class UserCustomSessionBlock
{
    private UserCustomSessionBlock() { }

    public UserCustomSessionBlock(long userCustomSessionId, string name, int sortOrder)
    {
        UserCustomSessionId = userCustomSessionId;
        Name = name;
        SortOrder = sortOrder;
    }

    public long Id { get; private set; }
    public long UserCustomSessionId { get; private set; }
    public string Name { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public UserCustomSession UserCustomSession { get; private set; } = null!;
}

