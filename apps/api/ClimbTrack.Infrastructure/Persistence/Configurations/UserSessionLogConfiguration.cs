using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class UserSessionLogConfiguration : IEntityTypeConfiguration<UserSessionLog>
{
    public void Configure(EntityTypeBuilder<UserSessionLog> builder)
    {
        builder.ToTable("user_session_logs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.UserPlanWeekId).HasColumnName("user_plan_week_id");
        builder.Property(x => x.SessionTypeId).HasColumnName("session_type_id").IsRequired();
        builder.Property(x => x.LogDate).HasColumnName("log_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.DayOfWeek).HasColumnName("day_of_week").IsRequired();
        builder.Property(x => x.IsDone).HasColumnName("is_done").IsRequired();
        builder.Property(x => x.Rpe).HasColumnName("rpe");
        builder.Property(x => x.DurationMin).HasColumnName("duration_min");
        builder.Property(x => x.Notes).HasColumnName("notes");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.SessionType)
            .WithMany()
            .HasForeignKey(x => x.SessionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Metrics)
            .WithOne(x => x.SessionLog)
            .HasForeignKey(x => x.SessionLogId);

        builder.HasIndex(x => new { x.UserId, x.LogDate });
    }
}

