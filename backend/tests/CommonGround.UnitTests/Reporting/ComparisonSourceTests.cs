using CommonGround.Modules.Reporting;
using CommonGround.Modules.Reporting.Entities;
using CommonGround.SharedKernel.Localization;
using FluentAssertions;

namespace CommonGround.UnitTests.Reporting;

// Covers ReflectionAssembler.GetComparisonSourceAsync (T009): the per-dimension building blocks
// the comparison assembler combines for two people. Unlike the reflection path, this keeps the
// full group/dimension structure so a side scoring below threshold still appears (null), which is
// what lets the engine surface one-sided differences.
public sealed class ComparisonSourceTests
{
    private static async Task<ReportingTestContext> SeedAsync(Action<ReportingTestContext> configure)
    {
        var db = ReportingTestContext.Create();
        configure(db);
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task No_Groups_Returns_Empty()
    {
        using var db = await SeedAsync(_ => { });
        var assembler = new ReflectionAssembler(db);

        var result = await assembler.GetComparisonSourceAsync(Guid.NewGuid(), SupportedLocales.Default);

        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public async Task Keeps_Below_Threshold_Dimension_With_Null_Strength_And_Text()
    {
        var responseSetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        using var db = await SeedAsync(ctx =>
        {
            ctx.DimensionScores.AddRange(
                new DimensionScore { Id = Guid.NewGuid(), ResponseSetId = responseSetId, DimensionId = "dim_high", RawScore = 4m, NormalisedScore = 0.8m },
                new DimensionScore { Id = Guid.NewGuid(), ResponseSetId = responseSetId, DimensionId = "dim_low", RawScore = 1m, NormalisedScore = 0.3m });
            ctx.InsightSnippets.AddRange(
                new InsightSnippet { Id = Guid.NewGuid(), DimensionId = "dim_high", Text = "High text" },
                new InsightSnippet { Id = Guid.NewGuid(), DimensionId = "dim_low", Text = "Low text" });
            ctx.DimensionGroups.Add(new DimensionGroup { Id = groupId, GroupId = "grp", Title = "Group", OrderIndex = 1 });
            ctx.DimensionGroupMemberships.AddRange(
                new DimensionGroupMembership { Id = Guid.NewGuid(), DimensionGroupId = groupId, DimensionId = "dim_high", OrderIndex = 1 },
                new DimensionGroupMembership { Id = Guid.NewGuid(), DimensionGroupId = groupId, DimensionId = "dim_low", OrderIndex = 2 });
        });

        var assembler = new ReflectionAssembler(db);
        var result = await assembler.GetComparisonSourceAsync(responseSetId, SupportedLocales.Default);

        var dims = result.Groups.Should().ContainSingle().Which.Dimensions;
        // Both dimensions present — the below-threshold one is kept (the comparison difference).
        dims.Should().HaveCount(2);

        var high = dims.Single(d => d.DimensionId == "dim_high");
        high.Strength.Should().Be(4);             // ceil(0.8 * 5)
        high.Text.Should().Be("High text");

        var low = dims.Single(d => d.DimensionId == "dim_low");
        low.Strength.Should().BeNull();
        low.Text.Should().BeNull();
    }

    [Fact]
    public async Task Uses_Requested_Locale_For_Title_And_Text_With_English_Fallback()
    {
        var responseSetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var snippetId = Guid.NewGuid();

        using var db = await SeedAsync(ctx =>
        {
            ctx.DimensionScores.Add(new DimensionScore { Id = Guid.NewGuid(), ResponseSetId = responseSetId, DimensionId = "dim_x", RawScore = 4m, NormalisedScore = 0.8m });
            ctx.InsightSnippets.Add(new InsightSnippet { Id = snippetId, DimensionId = "dim_x", Text = "English text" });
            ctx.InsightSnippetTranslations.Add(new InsightSnippetTranslation { Id = Guid.NewGuid(), InsightSnippetId = snippetId, Locale = "de", Text = "Deutscher Text" });
            ctx.DimensionTitles.AddRange(
                new DimensionTitle { Id = Guid.NewGuid(), DimensionId = "dim_x", Locale = "en", Title = "English title" },
                new DimensionTitle { Id = Guid.NewGuid(), DimensionId = "dim_x", Locale = "de", Title = "Deutscher Titel" });
            ctx.DimensionGroups.Add(new DimensionGroup { Id = groupId, GroupId = "grp", Title = "Group", OrderIndex = 1 });
            ctx.DimensionGroupTranslations.Add(new DimensionGroupTranslation { Id = Guid.NewGuid(), DimensionGroupId = groupId, Locale = "de", Title = "Gruppe" });
            ctx.DimensionGroupMemberships.Add(new DimensionGroupMembership { Id = Guid.NewGuid(), DimensionGroupId = groupId, DimensionId = "dim_x", OrderIndex = 1 });
        });

        var assembler = new ReflectionAssembler(db);

        var de = await assembler.GetComparisonSourceAsync(responseSetId, "de");
        var deGroup = de.Groups.Should().ContainSingle().Subject;
        deGroup.Title.Should().Be("Gruppe");
        var deDim = deGroup.Dimensions.Single();
        deDim.Title.Should().Be("Deutscher Titel");
        deDim.Text.Should().Be("Deutscher Text");

        var en = await assembler.GetComparisonSourceAsync(responseSetId, SupportedLocales.Default);
        var enGroup = en.Groups.Single();
        enGroup.Title.Should().Be("Group");
        enGroup.Dimensions.Single().Title.Should().Be("English title");
        enGroup.Dimensions.Single().Text.Should().Be("English text");
    }
}
