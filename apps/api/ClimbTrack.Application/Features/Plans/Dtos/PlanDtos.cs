namespace ClimbTrack.Application.Features.Plans.Dtos;

public record UserPlanDto(
    long Id,
    string Name,
    string TrainingTypeCode,
    string DifficultyLevelCode,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    double ProgressPct);

public record UserPlanDetailDto(
    long Id,
    string Name,
    string TrainingTypeCode,
    string DifficultyLevelCode,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    List<PlanWeekDto> Weeks);

public record PlanWeekDto(
    long Id,
    int WeekNumber,
    string PhaseName,
    string PhaseColorHex,
    double ProgressPct,
    bool IsDeload,
    List<DayEntryDto> Days);

public record DayEntryDto(
    string Label,
    string State,
    string? SessionTypeId,
    string? SessionTypeName,
    string? SessionColorHex);
