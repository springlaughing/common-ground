using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class DimensionGroupMembershipConfiguration : IEntityTypeConfiguration<DimensionGroupMembership>
{
    public void Configure(EntityTypeBuilder<DimensionGroupMembership> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DimensionId).HasMaxLength(100).IsRequired();
    }
}
