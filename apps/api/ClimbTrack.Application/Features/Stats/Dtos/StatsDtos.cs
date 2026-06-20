namespace ClimbTrack.Application.Features.Stats.Dtos;

public record StatsSummaryDto(
    int TotalSessions,
    int CurrentStreak,
    int MaxStreak,
    double RpeAverage,
    int SessionsThisWeek,
    int WeeksCompleted);

public record WeeklyLoadDto(
    string WeekLabel,
    int Arc,
    int Hangboard,
    int Campus,
    int Boulder,
    int Outdoor,
    int Other);

public record RpeDataPointDto(string Date, int Rpe, double MovingAvg);

public record EnergyDistributionDto(int Alactico, int Lactico, int Aerobico);
