namespace ClimbTrack.Domain.Entities;

public class UserPlan
{
    private UserPlan() { }

    public UserPlan(
        long userId,
        string name,
        int trainingTypeId,
        int difficultyLevelId,
        DateTime startDate,
        DateTime? endDate = null)
    {
        UserId = userId;
        Name = name;
        TrainingTypeId = trainingTypeId;
        DifficultyLevelId = difficultyLevelId;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public int TrainingTypeId { get; private set; }
    public int DifficultyLevelId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public TrainingType TrainingType { get; private set; } = null!;
    public DifficultyLevel DifficultyLevel { get; private set; } = null!;
    public ICollection<UserPlanWeek> Weeks { get; private set; } = [];
}

