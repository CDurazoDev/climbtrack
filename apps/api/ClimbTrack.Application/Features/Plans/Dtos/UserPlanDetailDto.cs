namespace ClimbTrack.Application.Features.Plans.Dtos;

public record UserPlanDetailDto(
    long Id,
    string Name,
    string TrainingTypeCode,
    string DifficultyLevelCode,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    List<PlanWeekDto> Weeks);
