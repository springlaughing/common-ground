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
        builder.Property(x => x.JoinedAt).IsRequired();
    }
}
