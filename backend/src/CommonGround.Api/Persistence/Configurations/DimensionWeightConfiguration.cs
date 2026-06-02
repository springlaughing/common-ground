using CommonGround.Modules.Questionnaires.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionWeightConfiguration : IEntityTypeConfiguration<DimensionWeight>
{
    public void Configure(EntityTypeBuilder<DimensionWeight> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DimensionId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Weight).IsRequired();
        builder.HasIndex(x => new { x.AnswerOptionId, x.DimensionId }).IsUnique();
    }
}
