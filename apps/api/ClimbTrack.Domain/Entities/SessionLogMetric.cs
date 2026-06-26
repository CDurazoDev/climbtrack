namespace ClimbTrack.Domain.Entities;

public class SessionLogMetric
{
    private SessionLogMetric() { }

    public SessionLogMetric(long sessionLogId, string metricKey, string metricValue, string? metricUnit = null)
    {
        SessionLogId = sessionLogId;
        MetricKey = metricKey;
        MetricValue = metricValue;
        MetricUnit = metricUnit;
    }

    public long Id { get; private set; }
    public long SessionLogId { get; private set; }
    public string MetricKey { get; private set; } = null!;
    public string MetricValue { get; private set; } = null!;
    public string? MetricUnit { get; private set; }
    public UserSessionLog SessionLog { get; private set; } = null!;

    public void Update(string metricValue, string? metricUnit)
    {
        MetricValue = metricValue;
        MetricUnit = metricUnit;
    }
}
