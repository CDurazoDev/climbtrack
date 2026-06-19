namespace ClimbTrack.Domain.Entities;

public class UserPlanWeek
{
    private UserPlanWeek() { }

    public UserPlanWeek(
        long userPlanId,
        int weekNumber,
        int phaseId,
        DateTime startDate,
        bool isDeload = false,
        double progressPct = 0,
        int? planTemplateId = null)
    {
        UserPlanId = userPlanId;
        WeekNumber = weekNumber;
        PhaseId = phaseId;
        StartDate = startDate;
        IsDeload = isDeload;
        ProgressPct = progressPct;
        PlanTemplateId = planTemplateId;
    }

    public long Id { get; private set; }
    public long UserPlanId { get; private set; }
    public int WeekNumber { get; private set; }
    public int PhaseId { get; private set; }
    public int? PlanTemplateId { get; private set; }
    public bool IsDeload { get; private set; }
    public double ProgressPct { get; private set; }
    public DateTime StartDate { get; private set; }
    public UserPlan Plan { get; private set; } = null!;
    public Phase Phase { get; private set; } = null!;
}

