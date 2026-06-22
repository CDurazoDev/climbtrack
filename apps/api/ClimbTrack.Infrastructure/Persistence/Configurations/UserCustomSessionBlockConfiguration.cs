using ClimbTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClimbTrack.Infrastructure.Persistence.Configurations;

public class UserCustomSessionBlockConfiguration : IEntityTypeConfiguration<UserCustomSessionBlock>
{
    public void Configure(EntityTypeBuilder<UserCustomSessionBlock> builder)
    {
        builder.ToTable("user_custom_session_blocks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserCustomSessionId).HasColumnName("user_custom_session_id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();
        builder.Property(x => x.ItemsJson).HasColumnName("items_json").HasColumnType("text").IsRequired();

        builder.HasIndex(x => new { x.UserCustomSessionId, x.SortOrder }).IsUnique();
    }
}
