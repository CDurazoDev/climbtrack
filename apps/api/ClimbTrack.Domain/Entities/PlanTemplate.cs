namespace ClimbTrack.Domain.Entities;

public class PlanTemplate
{
    private PlanTemplate() { }

    public PlanTemplate(
        string name,
        string source = "system",
        long? ownerUserId = null,
        int? phaseId = null,
        int? difficultyLevelId = null,
        bool isPublic = false)
    {
        Name = name;
        Source = source;
        OwnerUserId = ownerUserId;
        PhaseId = phaseId;
        DifficultyLevelId = difficultyLevelId;
        IsPublic = isPublic;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Source { get; private set; } = "system";
    public long? OwnerUserId { get; private set; }
    public int? PhaseId { get; private set; }
    public int? DifficultyLevelId { get; private set; }
    public bool IsPublic { get; private set; }
    public ICollection<PlanTemplateDay> Days { get; private set; } = [];
}

