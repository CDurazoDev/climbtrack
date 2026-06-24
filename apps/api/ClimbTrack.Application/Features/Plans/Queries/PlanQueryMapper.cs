using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Plans.Queries;

internal static class PlanQueryMapper
{
    private static readonly string[] DayLabels = ["L", "M", "X", "J", "V", "S", "D"];

    public static async Task<List<PlanWeekDto>> MapWeeksAsync(
        IApplicationDbContext db,
        long userId,
        IEnumerable<UserPlanWeek> weeks,
        CancellationToken cancellationToken)
    {
        var orderedWeeks = weeks.OrderBy(week => week.WeekNumber).ToList();
        if (orderedWeeks.Count == 0)
        {
            return [];
        }

        var weekIds = orderedWeeks.Select(week => week.Id).ToList();
        var templateIds = orderedWeeks
            .Where(week => week.PlanTemplateId.HasValue)
            .Select(week => week.PlanTemplateId!.Value)
            .Distinct()
            .ToList();

        var templateDays = templateIds.Count == 0
            ? []
            : await db.PlanTemplateDays
                .AsNoTracking()
                .Include(day => day.SessionType)
                .Where(day => templateIds.Contains(day.PlanTemplateId))
                .ToListAsync(cancellationToken);

        var logs = await db.UserSessionLogs
            .AsNoTracking()
            .Include(log => log.SessionType)
            .Where(log =>
                log.UserId == userId &&
                log.UserPlanWeekId.HasValue &&
                weekIds.Contains(log.UserPlanWeekId.Value))
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return orderedWeeks
            .Select(week => MapWeekDto(
                week,
                templateDays.Where(day => day.PlanTemplateId == week.PlanTemplateId).ToList(),
                logs.Where(log => log.UserPlanWeekId == week.Id).ToList(),
                today))
            .ToList();
    }

    private static PlanWeekDto MapWeekDto(
        UserPlanWeek week,
        List<PlanTemplateDay> templateDays,
        List<UserSessionLog> logs,
        DateOnly today)
    {
        var dayEntries = Enumerable.Range(0, 7)
            .Select(dayOfWeek => MapDayEntry(week, dayOfWeek, templateDays, logs, today))
            .ToList();

        return new PlanWeekDto(
            week.Id,
            week.WeekNumber,
            week.Phase.Name,
            PhaseColorHex(week.Phase.Code),
            week.ProgressPct,
            week.IsDeload,
            dayEntries);
    }

    private static DayEntryDto MapDayEntry(
        UserPlanWeek week,
        int dayOfWeek,
        List<PlanTemplateDay> templateDays,
        List<UserSessionLog> logs,
        DateOnly today)
    {
        var templateDay = templateDays.FirstOrDefault(day => day.DayOfWeek == dayOfWeek);
        var log = logs.FirstOrDefault(entry => entry.DayOfWeek == dayOfWeek);
        var dayDate = DateOnly.FromDateTime(week.StartDate).AddDays(dayOfWeek);

        if (templateDay?.IsRest == true || (templateDay is null && log is null))
        {
            return new DayEntryDto(DayLabels[dayOfWeek], "rest", null, null, null);
        }

        if (log?.IsDone == true)
        {
            return new DayEntryDto(
                DayLabels[dayOfWeek],
                "completed",
                ResolveSessionTypeId(templateDay, log),
                ResolveSessionTypeName(templateDay, log),
                ResolveSessionColorHex(templateDay, log));
        }

        var state = dayDate == today
            ? "today"
            : dayDate < today
                ? "failed"
                : "pending";

        return new DayEntryDto(
            DayLabels[dayOfWeek],
            state,
            ResolveSessionTypeId(templateDay, log),
            ResolveSessionTypeName(templateDay, log),
            ResolveSessionColorHex(templateDay, log));
    }

    private static string? ResolveSessionTypeId(PlanTemplateDay? templateDay, UserSessionLog? log)
    {
        return templateDay?.SessionTypeId?.ToString() ?? log?.SessionTypeId.ToString();
    }

    private static string? ResolveSessionTypeName(PlanTemplateDay? templateDay, UserSessionLog? log)
    {
        return templateDay?.SessionType?.Name ?? log?.SessionType.Name;
    }

    private static string? ResolveSessionColorHex(PlanTemplateDay? templateDay, UserSessionLog? log)
    {
        return templateDay?.SessionType?.ColorHex ?? log?.SessionType.ColorHex;
    }

    private static string PhaseColorHex(string phaseCode)
    {
        return phaseCode switch
        {
            "base" => "#4FC3F7",
            "fuerza" => "#FF6B35",
            "potencia" => "#FF4444",
            "resistencia" => "#66BB6A",
            "performance" => "#E8FF47",
            _ => "#A0A0A0"
        };
    }
}
