using CommonGround.Modules.Questionnaires.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class AnswerOptionConfiguration : IEntityTypeConfiguration<AnswerOption>
{
    public void Configure(EntityTypeBuilder<AnswerOption> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.OrderIndex).IsRequired();

        builder.HasMany(x => x.DimensionWeights)
            .WithOne(x => x.AnswerOption)
            .HasForeignKey(x => x.AnswerOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
