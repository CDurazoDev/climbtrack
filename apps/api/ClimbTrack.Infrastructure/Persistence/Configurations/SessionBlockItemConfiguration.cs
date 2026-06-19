using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class SessionBlockItemConfiguration : IEntityTypeConfiguration<SessionBlockItem>
{
    public void Configure(EntityTypeBuilder<SessionBlockItem> builder)
    {
        builder.ToTable("session_block_items");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SessionBlockId).HasColumnName("session_block_id").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasIndex(x => new { x.SessionBlockId, x.SortOrder }).IsUnique();
    }
}

