using CommonGround.Modules.Comparisons.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class InviteConfiguration : IEntityTypeConfiguration<Invite>
{
    public void Configure(EntityTypeBuilder<Invite> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InviterResponseSetId).IsRequired();
        builder.Property(x => x.InviterLabel).HasMaxLength(60).IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Validate/join looks the invite up by its token hash, which must be unique.
        builder.HasIndex(x => x.TokenHash).IsUnique();

        // An invite belongs to the session created alongside it; deleting the session removes it.
        builder.HasOne<ComparisonSession>()
            .WithMany()
            .HasForeignKey(x => x.ComparisonSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
