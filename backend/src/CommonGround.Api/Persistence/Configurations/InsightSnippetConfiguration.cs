using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class InsightSnippetConfiguration : IEntityTypeConfiguration<InsightSnippet>
{
    public void Configure(EntityTypeBuilder<InsightSnippet> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DimensionId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Text).HasMaxLength(2000).IsRequired();
    }
}
