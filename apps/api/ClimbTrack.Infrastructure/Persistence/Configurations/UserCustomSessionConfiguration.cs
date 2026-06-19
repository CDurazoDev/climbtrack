using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class UserCustomSessionConfiguration : IEntityTypeConfiguration<UserCustomSession>
{
    public void Configure(EntityTypeBuilder<UserCustomSession> builder)
    {
        builder.ToTable("user_custom_sessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(x => x.ColorHex).HasColumnName("color_hex").HasMaxLength(7).IsRequired();
        builder.Property(x => x.LoadLevel).HasColumnName("load_level").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

        builder.HasMany(x => x.Blocks)
            .WithOne(x => x.UserCustomSession)
            .HasForeignKey(x => x.UserCustomSessionId);
    }
}

