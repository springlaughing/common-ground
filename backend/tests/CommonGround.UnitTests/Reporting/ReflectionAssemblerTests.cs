using CommonGround.Modules.Reporting;
using CommonGround.Modules.Reporting.Entities;
using CommonGround.SharedKernel.Localization;
using FluentAssertions;

namespace CommonGround.UnitTests.Reporting;

public sealed class ReflectionAssemblerTests
{
    private static readonly Guid GroupId1 = Guid.NewGuid();
    private static readonly Guid GroupId2 = Guid.NewGuid();

    private static async Task<ReportingTestContext> SeedAsync(
        Action<ReportingTestContext> configure)
    {
        var db = ReportingTestContext.Create();
        configure(db);
        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Empty_Scores_Returns_Empty_Reflection()
    {
        using var db = await SeedAsync(_ => { });
        var assembler = new ReflectionAssembler(db);

        var result = await assembler.AssembleReflectionAsync(Guid.NewGuid(), SupportedLocales.Default);

        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public async Task Dimension_Below_Threshold_Is_Excluded()
    {
        var responseSetId = Guid.NewGuid();
        var groupEntityId = Guid.NewGuid();

        using var db = await SeedAsync(ctx =>
        {
            ctx.DimensionScores.Add(new DimensionScore
            {
                Id = Guid.NewGuid(),
                ResponseSetId = responseSetId,
                DimensionId = "dim_low",
                RawScore = 1m,
                NormalisedScore = 0.3m, // below 0.4 threshold
            });
            ctx.InsightSnippets.Add(new InsightSnippet
            {
                Id = Guid.NewGuid(), DimensionId = "dim_low", Text = "some text"
            });
            ctx.DimensionGroups.Add(new DimensionGroup
            {
                Id = groupEntityId, GroupId = "group_1", Title = "Group One", OrderIndex = 1
            });
            ctx.DimensionGroupMemberships.Add(new DimensionGroupMembership
            {
                Id = Guid.NewGuid(), DimensionGroupId = groupEntityId,
                DimensionId = "dim_low", OrderIndex = 1
            });
        });

        var assembler = new ReflectionAssembler(db);
        var result = await assembler.AssembleReflectionAsync(responseSetId, SupportedLocales.Default);

        result.Groups.Should().BeEmpty();
    }

    [Fact]
    public async Task Group_With_No_Qualifying_Dimensions_Is_Omitted()
    {
        var responseSetId = Guid.NewGuid();
        var groupWithQualifying = Guid.NewGuid();
        var groupWithout = Guid.NewGuid();

        using var db = await SeedAsync(ctx =>
        {
            ctx.DimensionScores.AddRange(
                new DimensionScore
                {
                    Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                    DimensionId = "dim_high", RawScore = 4m, NormalisedScore = 0.8m
                },
                new DimensionScore
                {
                    Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                    DimensionId = "dim_low", RawScore = 1m, NormalisedScore = 0.2m
                });

            ctx.InsightSnippets.AddRange(
                new InsightSnippet { Id = Guid.NewGuid(), DimensionId = "dim_high", Text = "High insight" },
                new InsightSnippet { Id = Guid.NewGuid(), DimensionId = "dim_low", Text = "Low insight" });

            ctx.DimensionGroups.AddRange(
                new DimensionGroup
                {
                    Id = groupWithQualifying, GroupId = "group_qualifying",
                    Title = "Has qualifying", OrderIndex = 1
                },
                new DimensionGroup
                {
                    Id = groupWithout, GroupId = "group_empty",
                    Title = "No qualifying", OrderIndex = 2
                });

            ctx.DimensionGroupMemberships.AddRange(
                new DimensionGroupMembership
                {
                    Id = Guid.NewGuid(), DimensionGroupId = groupWithQualifying,
                    DimensionId = "dim_high", OrderIndex = 1
                },
                new DimensionGroupMembership
                {
                    Id = Guid.NewGuid(), DimensionGroupId = groupWithout,
                    DimensionId = "dim_low", OrderIndex = 1
                });
        });

        var assembler = new ReflectionAssembler(db);
        var result = await assembler.AssembleReflectionAsync(responseSetId, SupportedLocales.Default);

        result.Groups.Should().ContainSingle()
            .Which.Id.Should().Be("group_qualifying");
    }

    [Fact]
    public async Task Strength_Is_Ceiling_Of_Score_Times_Five_Clamped_To_One_To_Five()
    {
        var responseSetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        // All scores >= 0.4 (display threshold) so all show in reflection
        var cases = new (string DimId, decimal Score, int ExpectedStrength)[]
        {
            ("dim_040", 0.40m, 2),  // ceil(0.40 * 5) = ceil(2.0)  = 2
            ("dim_060", 0.60m, 3),  // ceil(0.60 * 5) = ceil(3.0)  = 3
            ("dim_100", 1.00m, 5),  // ceil(1.00 * 5) = ceil(5.0)  = 5
        };

        using var db = await SeedAsync(ctx =>
        {
            var order = 1;
            foreach (var (dimId, score, _) in cases)
            {
                ctx.DimensionScores.Add(new DimensionScore
                {
                    Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                    DimensionId = dimId, RawScore = score, NormalisedScore = score
                });
                ctx.InsightSnippets.Add(new InsightSnippet
                {
                    Id = Guid.NewGuid(), DimensionId = dimId, Text = $"Insight for {dimId}"
                });
                ctx.DimensionGroupMemberships.Add(new DimensionGroupMembership
                {
                    Id = Guid.NewGuid(), DimensionGroupId = groupId,
                    DimensionId = dimId, OrderIndex = order++
                });
            }
            ctx.DimensionGroups.Add(new DimensionGroup
            {
                Id = groupId, GroupId = "group_strength", Title = "Strength Group", OrderIndex = 1
            });
        });

        var assembler = new ReflectionAssembler(db);
        var result = await assembler.AssembleReflectionAsync(responseSetId, SupportedLocales.Default);

        var insights = result.Groups.Should().ContainSingle().Which.Insights;
        foreach (var (dimId, _, expectedStrength) in cases)
        {
            insights.Should().ContainSingle(i => i.DimensionId == dimId)
                .Which.Strength.Should().Be(expectedStrength, $"dim {dimId}");
        }
    }

    [Fact]
    public async Task Insight_Text_Comes_From_Snippet_For_Dimension()
    {
        var responseSetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        const string expectedText = "You rely heavily on written structure.";

        using var db = await SeedAsync(ctx =>
        {
            ctx.DimensionScores.Add(new DimensionScore
            {
                Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                DimensionId = "dim_x", RawScore = 4m, NormalisedScore = 0.8m
            });
            ctx.InsightSnippets.Add(new InsightSnippet
            {
                Id = Guid.NewGuid(), DimensionId = "dim_x", Text = expectedText
            });
            ctx.DimensionGroups.Add(new DimensionGroup
            {
                Id = groupId, GroupId = "grp", Title = "Group", OrderIndex = 1
            });
            ctx.DimensionGroupMemberships.Add(new DimensionGroupMembership
            {
                Id = Guid.NewGuid(), DimensionGroupId = groupId,
                DimensionId = "dim_x", OrderIndex = 1
            });
        });

        var assembler = new ReflectionAssembler(db);
        var result = await assembler.AssembleReflectionAsync(responseSetId, SupportedLocales.Default);

        result.Groups.Single().Insights.Single().Text.Should().Be(expectedText);
    }

    [Fact]
    public async Task Insight_Title_Uses_Requested_Locale_With_English_Fallback()
    {
        var responseSetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        using var db = await SeedAsync(ctx =>
        {
            // dim_both has en + de titles; dim_en_only has just en (exercises the fallback).
            foreach (var dim in new[] { "dim_both", "dim_en_only" })
            {
                ctx.DimensionScores.Add(new DimensionScore
                {
                    Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                    DimensionId = dim, RawScore = 4m, NormalisedScore = 0.8m
                });
                ctx.InsightSnippets.Add(new InsightSnippet
                {
                    Id = Guid.NewGuid(), DimensionId = dim, Text = $"Insight for {dim}"
                });
                ctx.DimensionGroupMemberships.Add(new DimensionGroupMembership
                {
                    Id = Guid.NewGuid(), DimensionGroupId = groupId, DimensionId = dim, OrderIndex = 1
                });
            }
            ctx.DimensionGroups.Add(new DimensionGroup
            {
                Id = groupId, GroupId = "grp", Title = "Group", OrderIndex = 1
            });
            ctx.DimensionTitles.AddRange(
                new DimensionTitle { Id = Guid.NewGuid(), DimensionId = "dim_both", Locale = "en", Title = "Written records over memory" },
                new DimensionTitle { Id = Guid.NewGuid(), DimensionId = "dim_both", Locale = "de", Title = "Schriftliches vor Erinnerung" },
                new DimensionTitle { Id = Guid.NewGuid(), DimensionId = "dim_en_only", Locale = "en", Title = "Real examples over descriptions" });
        });

        var assembler = new ReflectionAssembler(db);

        var de = await assembler.AssembleReflectionAsync(responseSetId, "de");
        var deInsights = de.Groups.Single().Insights;
        deInsights.Single(i => i.DimensionId == "dim_both").Title.Should().Be("Schriftliches vor Erinnerung");
        deInsights.Single(i => i.DimensionId == "dim_en_only").Title.Should().Be("Real examples over descriptions");

        var en = await assembler.AssembleReflectionAsync(responseSetId, SupportedLocales.Default);
        en.Groups.Single().Insights.Single(i => i.DimensionId == "dim_both").Title.Should().Be("Written records over memory");
    }

    [Fact]
    public async Task Groups_Are_Ordered_By_OrderIndex()
    {
        var responseSetId = Guid.NewGuid();
        var group2Id = Guid.NewGuid();
        var group1Id = Guid.NewGuid();

        using var db = await SeedAsync(ctx =>
        {
            ctx.DimensionScores.AddRange(
                new DimensionScore
                {
                    Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                    DimensionId = "dim_alpha", RawScore = 1m, NormalisedScore = 0.5m
                },
                new DimensionScore
                {
                    Id = Guid.NewGuid(), ResponseSetId = responseSetId,
                    DimensionId = "dim_beta", RawScore = 1m, NormalisedScore = 0.5m
                });
            ctx.InsightSnippets.AddRange(
                new InsightSnippet { Id = Guid.NewGuid(), DimensionId = "dim_alpha", Text = "Alpha insight" },
                new InsightSnippet { Id = Guid.NewGuid(), DimensionId = "dim_beta", Text = "Beta insight" });

            // Add groups with reversed insertion order vs OrderIndex
            ctx.DimensionGroups.AddRange(
                new DimensionGroup
                {
                    Id = group2Id, GroupId = "group_second", Title = "Second", OrderIndex = 2
                },
                new DimensionGroup
                {
                    Id = group1Id, GroupId = "group_first", Title = "First", OrderIndex = 1
                });
            ctx.DimensionGroupMemberships.AddRange(
                new DimensionGroupMembership
                {
                    Id = Guid.NewGuid(), DimensionGroupId = group1Id,
                    DimensionId = "dim_alpha", OrderIndex = 1
                },
                new DimensionGroupMembership
                {
                    Id = Guid.NewGuid(), DimensionGroupId = group2Id,
                    DimensionId = "dim_beta", OrderIndex = 1
                });
        });

        var assembler = new ReflectionAssembler(db);
        var result = await assembler.AssembleReflectionAsync(responseSetId, SupportedLocales.Default);

        result.Groups.Should().HaveCount(2);
        result.Groups[0].Id.Should().Be("group_first");
        result.Groups[1].Id.Should().Be("group_second");
    }
}
