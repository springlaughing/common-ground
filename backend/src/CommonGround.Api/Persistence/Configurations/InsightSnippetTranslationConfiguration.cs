using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class InsightSnippetTranslationConfiguration : IEntityTypeConfiguration<InsightSnippetTranslation>
{
    public void Configure(EntityTypeBuilder<InsightSnippetTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Locale).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Text).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.InsightSnippetId, x.Locale }).IsUnique();

        builder.HasOne<InsightSnippet>()
            .WithMany()
            .HasForeignKey(x => x.InsightSnippetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
