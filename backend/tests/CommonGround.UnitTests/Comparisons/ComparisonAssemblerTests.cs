using CommonGround.Modules.Comparisons.Services;
using CommonGround.SharedKernel.Interfaces;
using FluentAssertions;
using Moq;

namespace CommonGround.UnitTests.Comparisons;

/// <summary>
/// T035 (US4) — the assembler combines the engine's classification with Reporting's localized
/// per-dimension title/text: differences before similarities within a group, omitted dimensions
/// dropped, each side's strength/text mapped to you/them, and the requested locale passed through to
/// Reporting (whose source already applies the English fallback).
/// </summary>
public sealed class ComparisonAssemblerTests
{
    private static readonly Guid Viewer = Guid.NewGuid();
    private static readonly Guid Other = Guid.NewGuid();

    private static ComparisonSourceDto Source(params ComparisonSourceDimensionDto[] dims) =>
        new([new ComparisonSourceGroupDto("g1", "Group One", dims)]);

    private static (ComparisonAssembler Assembler, Mock<IReportingService> Reporting) Build(
        ComparisonSourceDto viewerSource, ComparisonSourceDto otherSource, string locale = "en")
    {
        var reporting = new Mock<IReportingService>();
        reporting.Setup(r => r.GetComparisonSourceAsync(Viewer, locale, It.IsAny<CancellationToken>())).ReturnsAsync(viewerSource);
        reporting.Setup(r => r.GetComparisonSourceAsync(Other, locale, It.IsAny<CancellationToken>())).ReturnsAsync(otherSource);
        return (new ComparisonAssembler(reporting.Object), reporting);
    }

    [Fact]
    public async Task Assemble_OrdersDifferencesBeforeSimilarities_AndDropsOmitted()
    {
        var viewer = Source(
            new ComparisonSourceDimensionDto("dSim", "Sim Title", 3, "you sim"),
            new ComparisonSourceDimensionDto("dDiff", "Diff Title", 5, "you diff"),
            new ComparisonSourceDimensionDto("dOmit", "Omit Title", null, null));
        var other = Source(
            new ComparisonSourceDimensionDto("dSim", "Sim Title", 3, "them sim"),
            new ComparisonSourceDimensionDto("dDiff", "Diff Title", 1, "them diff"),
            new ComparisonSourceDimensionDto("dOmit", "Omit Title", null, null));
        var (assembler, _) = Build(viewer, other);

        var report = await assembler.AssembleAsync(Viewer, Other, "Sam", "en");

        report.OtherLabel.Should().Be("Sam");
        var group = report.Groups.Should().ContainSingle().Subject;
        // dOmit dropped; difference before similarity.
        group.Insights.Select(i => i.DimensionId).Should().Equal("dDiff", "dSim");
        group.Insights.Select(i => i.Classification).Should().Equal("difference", "similarity");
    }

    [Fact]
    public async Task Assemble_MapsStrengthsAndTextToYouAndThem()
    {
        var viewer = Source(new ComparisonSourceDimensionDto("d", "Title", 5, "your text"));
        var other = Source(new ComparisonSourceDimensionDto("d", "Title", 1, "their text"));
        var (assembler, _) = Build(viewer, other);

        var report = await assembler.AssembleAsync(Viewer, Other, "Sam", "en");

        var insight = report.Groups.Single().Insights.Single();
        insight.Title.Should().Be("Title");
        insight.YourStrength.Should().Be(5);
        insight.TheirStrength.Should().Be(1);
        insight.YourText.Should().Be("your text");
        insight.TheirText.Should().Be("their text");
        insight.Classification.Should().Be("difference");
    }

    [Fact]
    public async Task Assemble_OneSidedDimension_IsADifference_WithNullStrengthForTheAbsentSide()
    {
        var viewer = Source(new ComparisonSourceDimensionDto("d", "Title", 4, "your text"));
        var other = Source(new ComparisonSourceDimensionDto("d", "Title", null, null));
        var (assembler, _) = Build(viewer, other);

        var report = await assembler.AssembleAsync(Viewer, Other, "Sam", "en");

        var insight = report.Groups.Single().Insights.Single();
        insight.Classification.Should().Be("difference");
        insight.YourStrength.Should().Be(4);
        insight.TheirStrength.Should().BeNull();
        insight.TheirText.Should().BeNull();
    }

    [Fact]
    public async Task Assemble_PassesRequestedLocaleThrough_ToReporting()
    {
        var viewer = Source(new ComparisonSourceDimensionDto("d", "Titel", 3, "dein Text"));
        var other = Source(new ComparisonSourceDimensionDto("d", "Titel", 3, "ihr Text"));
        var (assembler, reporting) = Build(viewer, other, "de");

        var report = await assembler.AssembleAsync(Viewer, Other, "Sam", "de");

        report.Groups.Single().Insights.Single().Title.Should().Be("Titel");
        reporting.Verify(r => r.GetComparisonSourceAsync(Viewer, "de", It.IsAny<CancellationToken>()), Times.Once);
        reporting.Verify(r => r.GetComparisonSourceAsync(Other, "de", It.IsAny<CancellationToken>()), Times.Once);
    }
}
