using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class SessionTypeConfiguration : IEntityTypeConfiguration<SessionType>
{
    public void Configure(EntityTypeBuilder<SessionType> builder)
    {
        builder.ToTable("session_types");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ColorHex).HasColumnName("color_hex").HasMaxLength(7).IsRequired();
        builder.Property(x => x.LoadLevel).HasColumnName("load_level").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.EnergySystemId).HasColumnName("energy_system_id").IsRequired();

        builder.HasOne(x => x.EnergySystem)
            .WithMany()
            .HasForeignKey(x => x.EnergySystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Blocks)
            .WithOne(x => x.SessionType)
            .HasForeignKey(x => x.SessionTypeId);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

