using CommonGround.Modules.Questionnaires.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionMaxScoreConfiguration : IEntityTypeConfiguration<DimensionMaxScore>
{
    public void Configure(EntityTypeBuilder<DimensionMaxScore> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DimensionId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MaxScore).HasPrecision(10, 4).IsRequired();
        builder.HasIndex(x => new { x.QuestionnaireVersionId, x.DimensionId }).IsUnique();
    }
}
