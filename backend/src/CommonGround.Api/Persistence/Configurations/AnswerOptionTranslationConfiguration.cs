using CommonGround.Modules.Questionnaires.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class AnswerOptionTranslationConfiguration : IEntityTypeConfiguration<AnswerOptionTranslation>
{
    public void Configure(EntityTypeBuilder<AnswerOptionTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Locale).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Text).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.AnswerOptionId, x.Locale }).IsUnique();

        builder.HasOne<AnswerOption>()
            .WithMany()
            .HasForeignKey(x => x.AnswerOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
