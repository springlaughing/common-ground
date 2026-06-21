using CommonGround.Modules.Comparisons.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class ComparisonParticipantConfiguration : IEntityTypeConfiguration<ComparisonParticipant>
{
    public void Configure(EntityTypeBuilder<ComparisonParticipant> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.DisplayLabel).HasMaxLength(60).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();

        // The /me hub lists a person's comparisons by their ResponseSet.
        builder.HasIndex(x => x.ResponseSetId);

        // A response joins a given comparison at most once.
        builder.HasIndex(x => new { x.ComparisonSessionId, x.ResponseSetId }).IsUnique();
    }
}
