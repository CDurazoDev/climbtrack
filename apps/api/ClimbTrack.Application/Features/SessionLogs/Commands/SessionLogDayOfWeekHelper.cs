namespace ClimbTrack.Application.Features.SessionLogs.Commands;

internal static class SessionLogDayOfWeekHelper
{
    public static int ToClimbTrackDayOfWeek(DateOnly date)
    {
        return ((int)date.DayOfWeek + 6) % 7;
    }
}
