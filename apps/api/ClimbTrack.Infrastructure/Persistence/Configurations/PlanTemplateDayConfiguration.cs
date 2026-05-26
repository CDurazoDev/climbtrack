using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class PlanTemplateDayConfiguration : IEntityTypeConfiguration<PlanTemplateDay>
{
    public void Configure(EntityTypeBuilder<PlanTemplateDay> builder)
    {
        builder.ToTable("plan_template_days");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PlanTemplateId).HasColumnName("plan_template_id").IsRequired();
        builder.Property(x => x.DayOfWeek).HasColumnName("day_of_week").IsRequired();
        builder.Property(x => x.SessionTypeId).HasColumnName("session_type_id");
        builder.Property(x => x.IsRest).HasColumnName("is_rest").IsRequired();

        builder.HasOne(x => x.SessionType)
            .WithMany()
            .HasForeignKey(x => x.SessionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PlanTemplateId, x.DayOfWeek }).IsUnique();
    }
}

