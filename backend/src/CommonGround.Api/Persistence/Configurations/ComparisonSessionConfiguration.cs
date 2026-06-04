using CommonGround.Modules.Comparisons.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class ComparisonSessionConfiguration : IEntityTypeConfiguration<ComparisonSession>
{
    public void Configure(EntityTypeBuilder<ComparisonSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Participants)
            .WithOne(x => x.ComparisonSession)
            .HasForeignKey(x => x.ComparisonSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
