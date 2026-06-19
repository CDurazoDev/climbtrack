using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class UserPlanWeekConfiguration : IEntityTypeConfiguration<UserPlanWeek>
{
    public void Configure(EntityTypeBuilder<UserPlanWeek> builder)
    {
        builder.ToTable("user_plan_weeks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserPlanId).HasColumnName("user_plan_id").IsRequired();
        builder.Property(x => x.WeekNumber).HasColumnName("week_number").IsRequired();
        builder.Property(x => x.PhaseId).HasColumnName("phase_id").IsRequired();
        builder.Property(x => x.PlanTemplateId).HasColumnName("plan_template_id");
        builder.Property(x => x.IsDeload).HasColumnName("is_deload").IsRequired();
        builder.Property(x => x.ProgressPct).HasColumnName("progress_pct").HasColumnType("double precision").IsRequired();
        builder.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();

        builder.HasOne(x => x.Phase)
            .WithMany()
            .HasForeignKey(x => x.PhaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserPlanId, x.WeekNumber }).IsUnique();
    }
}

