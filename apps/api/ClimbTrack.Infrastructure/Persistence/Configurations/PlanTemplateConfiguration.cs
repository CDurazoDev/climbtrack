using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class PlanTemplateConfiguration : IEntityTypeConfiguration<PlanTemplate>
{
    public void Configure(EntityTypeBuilder<PlanTemplate> builder)
    {
        builder.ToTable("plan_templates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(20).IsRequired();
        builder.Property(x => x.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(x => x.PhaseId).HasColumnName("phase_id");
        builder.Property(x => x.DifficultyLevelId).HasColumnName("difficulty_level_id");
        builder.Property(x => x.IsPublic).HasColumnName("is_public").IsRequired();

        builder.HasMany(x => x.Days)
            .WithOne(x => x.Template)
            .HasForeignKey(x => x.PlanTemplateId);
    }
}

