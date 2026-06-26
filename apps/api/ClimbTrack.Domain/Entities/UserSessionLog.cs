namespace ClimbTrack.Domain.Entities;

public class UserSessionLog
{
    private UserSessionLog() { }

    public UserSessionLog(
        long userId,
        int sessionTypeId,
        DateOnly logDate,
        int dayOfWeek,
        bool isDone,
        int? rpe = null,
        int? durationMin = null,
        string? notes = null,
        long? userPlanWeekId = null)
    {
        UserId = userId;
        SessionTypeId = sessionTypeId;
        LogDate = logDate;
        DayOfWeek = dayOfWeek;
        IsDone = isDone;
        Rpe = rpe;
        DurationMin = durationMin;
        Notes = notes;
        UserPlanWeekId = userPlanWeekId;
        CreatedAt = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long? UserPlanWeekId { get; private set; }
    public int SessionTypeId { get; private set; }
    public DateOnly LogDate { get; private set; }
    public int DayOfWeek { get; private set; }
    public bool IsDone { get; private set; }
    public int? Rpe { get; private set; }
    public int? DurationMin { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public SessionType SessionType { get; private set; } = null!;
    public ICollection<SessionLogMetric> Metrics { get; private set; } = [];

    public void UpdateDraft(int? rpe, int? durationMin, string? notes)
    {
        Rpe = rpe;
        DurationMin = durationMin;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(int rpe, int durationMin, string? notes)
    {
        IsDone = true;
        Rpe = rpe;
        DurationMin = durationMin;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

