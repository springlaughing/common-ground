using CommonGround.Modules.Questionnaires.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class QuestionnaireVersionConfiguration : IEntityTypeConfiguration<QuestionnaireVersion>
{
    public void Configure(EntityTypeBuilder<QuestionnaireVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VersionNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Questions)
            .WithOne(x => x.QuestionnaireVersion)
            .HasForeignKey(x => x.QuestionnaireVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DimensionMaxScores)
            .WithOne(x => x.QuestionnaireVersion)
            .HasForeignKey(x => x.QuestionnaireVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
