using CommonGround.Modules.Responses.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommonGround.Api.Persistence.Configurations;

internal sealed class ResponseSetConfiguration : IEntityTypeConfiguration<ResponseSet>
{
    public void Configure(EntityTypeBuilder<ResponseSet> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrivateResultTokenHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.AccessCodeHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Answers)
            .WithOne(x => x.ResponseSet)
            .HasForeignKey(x => x.ResponseSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
