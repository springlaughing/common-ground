using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionGroupTranslationConfiguration : IEntityTypeConfiguration<DimensionGroupTranslation>
{
    public void Configure(EntityTypeBuilder<DimensionGroupTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Locale).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.DimensionGroupId, x.Locale }).IsUnique();

        builder.HasOne<DimensionGroup>()
            .WithMany()
            .HasForeignKey(x => x.DimensionGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
