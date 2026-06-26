using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Entities;

namespace ClimbTrack.Application.Features.SessionLogs.Commands;

internal static class SessionLogWriteHelper
{
    public static void SyncMetrics(
        IApplicationDbContext db,
        UserSessionLog sessionLog,
        IReadOnlyCollection<MetricInput> metrics)
    {
        var incomingMetrics = metrics
            .GroupBy(metric => metric.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToDictionary(metric => metric.Key, StringComparer.OrdinalIgnoreCase);

        var metricsToRemove = sessionLog.Metrics
            .Where(existingMetric => !incomingMetrics.ContainsKey(existingMetric.MetricKey))
            .ToList();

        if (metricsToRemove.Count > 0)
        {
            db.SessionLogMetrics.RemoveRange(metricsToRemove);
        }

        foreach (var metric in incomingMetrics.Values)
        {
            var existingMetric = sessionLog.Metrics
                .FirstOrDefault(entry =>
                    string.Equals(entry.MetricKey, metric.Key, StringComparison.OrdinalIgnoreCase));

            if (existingMetric is null)
            {
                sessionLog.Metrics.Add(new SessionLogMetric(
                    sessionLog.Id,
                    metric.Key,
                    metric.Value,
                    metric.Unit));

                continue;
            }

            existingMetric.Update(metric.Value, metric.Unit);
        }
    }

    public static SessionLogDto ToDto(UserSessionLog sessionLog)
    {
        return new SessionLogDto(
            sessionLog.Id,
            sessionLog.LogDate,
            sessionLog.SessionTypeId.ToString(),
            sessionLog.SessionType.Name,
            sessionLog.SessionType.ColorHex,
            sessionLog.IsDone,
            sessionLog.Rpe,
            sessionLog.DurationMin,
            sessionLog.Notes);
    }
}
