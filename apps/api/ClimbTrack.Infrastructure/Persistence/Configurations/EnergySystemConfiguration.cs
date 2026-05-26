using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class EnergySystemConfiguration : IEntityTypeConfiguration<EnergySystem>
{
    public void Configure(EntityTypeBuilder<EnergySystem> builder)
    {
        builder.ToTable("energy_systems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DurationRange).HasColumnName("duration_range").HasMaxLength(50);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

