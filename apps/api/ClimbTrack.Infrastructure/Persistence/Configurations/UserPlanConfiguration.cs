using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class UserPlanConfiguration : IEntityTypeConfiguration<UserPlan>
{
    public void Configure(EntityTypeBuilder<UserPlan> builder)
    {
        builder.ToTable("user_plans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(x => x.TrainingTypeId).HasColumnName("training_type_id").IsRequired();
        builder.Property(x => x.DifficultyLevelId).HasColumnName("difficulty_level_id").IsRequired();
        builder.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.TrainingType)
            .WithMany()
            .HasForeignKey(x => x.TrainingTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DifficultyLevel)
            .WithMany()
            .HasForeignKey(x => x.DifficultyLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Weeks)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.UserPlanId);

        builder.HasIndex(x => new { x.UserId, x.IsActive })
            .HasFilter("is_active = true")
            .IsUnique();
    }
}

