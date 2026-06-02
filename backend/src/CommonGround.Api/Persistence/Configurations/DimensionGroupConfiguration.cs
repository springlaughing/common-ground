using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionGroupConfiguration : IEntityTypeConfiguration<DimensionGroup>
{
    public void Configure(EntityTypeBuilder<DimensionGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GroupId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.GroupId).IsUnique();

        builder.HasMany(x => x.Memberships)
            .WithOne(x => x.DimensionGroup)
            .HasForeignKey(x => x.DimensionGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
