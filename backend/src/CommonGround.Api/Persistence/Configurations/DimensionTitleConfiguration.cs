using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionTitleConfiguration : IEntityTypeConfiguration<DimensionTitle>
{
    public void Configure(EntityTypeBuilder<DimensionTitle> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DimensionId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Locale).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();

        // Locale-first: one row per (DimensionId, Locale), English included.
        builder.HasIndex(x => new { x.DimensionId, x.Locale }).IsUnique();
    }
}
