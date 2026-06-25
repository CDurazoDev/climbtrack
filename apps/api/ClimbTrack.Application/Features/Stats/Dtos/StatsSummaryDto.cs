namespace ClimbTrack.Application.Features.Stats.Dtos;

public sealed record StatsSummaryDto(
    int TotalSessions,
    int CurrentStreak,
    int MaxStreak,
    double RpeAverage,
    int SessionsThisWeek,
    int WeeksCompleted);
