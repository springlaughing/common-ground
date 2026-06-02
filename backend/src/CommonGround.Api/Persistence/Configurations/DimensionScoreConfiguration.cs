using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionScoreConfiguration : IEntityTypeConfiguration<DimensionScore>
{
    public void Configure(EntityTypeBuilder<DimensionScore> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DimensionId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RawScore).HasPrecision(10, 4).IsRequired();
        builder.Property(x => x.NormalisedScore).HasPrecision(6, 4).IsRequired();
        builder.HasIndex(x => new { x.ResponseSetId, x.DimensionId }).IsUnique();
    }
}
