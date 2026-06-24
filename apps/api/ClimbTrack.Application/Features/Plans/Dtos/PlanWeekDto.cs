namespace ClimbTrack.Application.Features.Plans.Dtos;

public record PlanWeekDto(
    long Id,
    int WeekNumber,
    string PhaseName,
    string PhaseColorHex,
    double ProgressPct,
    bool IsDeload,
    List<DayEntryDto> Days);
