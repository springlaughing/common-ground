using CommonGround.Modules.Reporting.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.UnitTests.Reporting;

internal sealed class ReportingTestContext : DbContext
{
    internal ReportingTestContext(DbContextOptions<ReportingTestContext> options) : base(options) { }

    public DbSet<DimensionScore> DimensionScores => Set<DimensionScore>();
    public DbSet<InsightSnippet> InsightSnippets => Set<InsightSnippet>();
    public DbSet<DimensionGroup> DimensionGroups => Set<DimensionGroup>();
    public DbSet<DimensionGroupMembership> DimensionGroupMemberships => Set<DimensionGroupMembership>();
    public DbSet<DimensionTitle> DimensionTitles => Set<DimensionTitle>();
    public DbSet<InsightSnippetTranslation> InsightSnippetTranslations => Set<InsightSnippetTranslation>();
    public DbSet<DimensionGroupTranslation> DimensionGroupTranslations => Set<DimensionGroupTranslation>();

    internal static ReportingTestContext Create() =>
        new(new DbContextOptionsBuilder<ReportingTestContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
