using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class SessionLogMetricConfiguration : IEntityTypeConfiguration<SessionLogMetric>
{
    public void Configure(EntityTypeBuilder<SessionLogMetric> builder)
    {
        builder.ToTable("session_log_metrics");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SessionLogId).HasColumnName("session_log_id").IsRequired();
        builder.Property(x => x.MetricKey).HasColumnName("metric_key").HasMaxLength(50).IsRequired();
        builder.Property(x => x.MetricValue).HasColumnName("metric_value").IsRequired();
        builder.Property(x => x.MetricUnit).HasColumnName("metric_unit").HasMaxLength(20);

        builder.HasIndex(x => new { x.SessionLogId, x.MetricKey }).IsUnique();
    }
}

