using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class SessionBlockConfiguration : IEntityTypeConfiguration<SessionBlock>
{
    public void Configure(EntityTypeBuilder<SessionBlock> builder)
    {
        builder.ToTable("session_blocks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SessionTypeId).HasColumnName("session_type_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Block)
            .HasForeignKey(x => x.SessionBlockId);

        builder.HasIndex(x => new { x.SessionTypeId, x.SortOrder }).IsUnique();
    }
}

