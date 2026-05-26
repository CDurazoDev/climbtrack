namespace ClimbTrack.Domain.Entities;

public class PlanTemplateDay
{
    private PlanTemplateDay() { }

    public PlanTemplateDay(int planTemplateId, int dayOfWeek, bool isRest, int? sessionTypeId = null)
    {
        PlanTemplateId = planTemplateId;
        DayOfWeek = dayOfWeek;
        SessionTypeId = sessionTypeId;
        IsRest = isRest;
    }

    public int Id { get; private set; }
    public int PlanTemplateId { get; private set; }
    public int DayOfWeek { get; private set; }
    public int? SessionTypeId { get; private set; }
    public bool IsRest { get; private set; }
    public PlanTemplate Template { get; private set; } = null!;
    public SessionType? SessionType { get; private set; }
}

