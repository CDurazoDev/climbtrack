namespace ClimbTrack.Application.Features.Stats.Dtos;

public record StatsSummaryDto(
    int TotalSessions,
    int CurrentStreak,
    int MaxStreak,
    double RpeAverage,
    int SessionsThisWeek,
    int WeeksCompleted);

public record WeeklyLoadDto(
    string Label,
    int Arc,
    int Hangboard,
    int Campus,
    int Boulder,
    int Outdoor);

public record RpeDataPointDto(
    string Label,
    double Rpe,
    double AvgRpe);

public record EnergyDistributionDto(
    double Alactico,
    double Aerobico,
    double Lactico);

public record SessionHistoryDto(
    long Id,
    DateOnly LogDate,
    string SessionTypeId,
    string SessionTypeName,
    string SessionColorHex,
    int? Rpe,
    int? DurationMin);
