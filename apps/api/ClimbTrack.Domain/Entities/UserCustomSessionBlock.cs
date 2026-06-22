namespace ClimbTrack.Domain.Entities;

using System.Text.Json;

public class UserCustomSessionBlock
{
    private UserCustomSessionBlock() { }

    public UserCustomSessionBlock(
        long userCustomSessionId,
        string name,
        int sortOrder,
        IEnumerable<string>? items = null)
    {
        UserCustomSessionId = userCustomSessionId;
        Name = name;
        SortOrder = sortOrder;
        SetItems(items ?? []);
    }

    public long Id { get; private set; }
    public long UserCustomSessionId { get; private set; }
    public string Name { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public string ItemsJson { get; private set; } = "[]";
    public UserCustomSession UserCustomSession { get; private set; } = null!;

    public IReadOnlyList<string> GetItems()
    {
        return JsonSerializer.Deserialize<List<string>>(ItemsJson) ?? [];
    }

    public void Update(string name, int sortOrder, IEnumerable<string> items)
    {
        Name = name;
        SortOrder = sortOrder;
        SetItems(items);
    }

    private void SetItems(IEnumerable<string> items)
    {
        var sanitized = items
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();

        ItemsJson = JsonSerializer.Serialize(sanitized);
    }
}

