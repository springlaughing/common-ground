using CommonGround.Modules.Comparisons.Services;
using FluentAssertions;

namespace CommonGround.UnitTests.Comparisons;

/// <summary>
/// T030 (US3) — exhaustive coverage of the deterministic comparison engine (compensates for the
/// blocked Stryker run, per the constitution): every classification branch and gap boundary, the
/// differences-then-similarities ordering, symmetry (A↔B equals B↔A per viewpoint), and determinism.
/// </summary>
public sealed class ComparisonEngineTests
{
    private static DimensionStrengths Dim(string id, int? yours, int? theirs) => new(id, yours, theirs);

    // ── Classification: every branch and both gap boundaries ──────────────────

    [Theory]
    [InlineData(3, 3)] // gap 0
    [InlineData(4, 3)] // gap 1 (boundary — still aligned)
    [InlineData(1, 2)]
    [InlineData(5, 5)]
    public void BothShown_WithinGap_IsSimilarity(int yours, int theirs)
    {
        var result = ComparisonEngine.Compare([Dim("d", yours, theirs)]);

        result.Should().ContainSingle()
            .Which.Kind.Should().Be(DimensionComparisonKind.Similarity);
    }

    [Theory]
    [InlineData(5, 3)] // gap 2 (boundary — now a difference)
    [InlineData(5, 1)] // gap 4
    [InlineData(2, 5)]
    public void BothShown_BeyondGap_IsDifference(int yours, int theirs)
    {
        var result = ComparisonEngine.Compare([Dim("d", yours, theirs)]);

        result.Should().ContainSingle()
            .Which.Kind.Should().Be(DimensionComparisonKind.Difference);
    }

    [Theory]
    [InlineData(4, null)] // only you show it
    [InlineData(null, 4)] // only they show it
    public void ExactlyOneShown_IsDifference(int? yours, int? theirs)
    {
        var result = ComparisonEngine.Compare([Dim("d", yours, theirs)]);

        result.Should().ContainSingle()
            .Which.Kind.Should().Be(DimensionComparisonKind.Difference);
    }

    [Fact]
    public void BothBelowThreshold_IsOmitted()
    {
        var result = ComparisonEngine.Compare([Dim("d", null, null)]);

        result.Should().BeEmpty();
    }

    // ── Ordering: differences first, then similarities; stable within each ─────

    [Fact]
    public void Orders_DifferencesFirst_ThenSimilarities_PreservingInputOrderWithinEach()
    {
        var result = ComparisonEngine.Compare(
        [
            Dim("simA", 3, 3),   // similarity
            Dim("difB", 5, 1),   // difference
            Dim("simC", 2, 2),   // similarity
            Dim("difD", 4, null),// difference
            Dim("omit", null, null), // dropped
        ]);

        result.Select(c => c.DimensionId).Should().Equal("difB", "difD", "simA", "simC");
    }

    // ── Symmetry: A↔B equals B↔A per viewpoint ─────────────────────────────────

    [Fact]
    public void IsSymmetric_SwappingParticipants_SwapsStrengthsButKeepsKindsAndOrder()
    {
        var ab = ComparisonEngine.Compare(
        [
            Dim("d1", 5, 1),
            Dim("d2", 3, 3),
            Dim("d3", null, 4),
        ]);
        var ba = ComparisonEngine.Compare(
        [
            Dim("d1", 1, 5),
            Dim("d2", 3, 3),
            Dim("d3", 4, null),
        ]);

        ba.Select(c => c.DimensionId).Should().Equal(ab.Select(c => c.DimensionId));
        ba.Select(c => c.Kind).Should().Equal(ab.Select(c => c.Kind));
        foreach (var a in ab)
        {
            var b = ba.Single(x => x.DimensionId == a.DimensionId);
            b.YourStrength.Should().Be(a.TheirStrength);
            b.TheirStrength.Should().Be(a.YourStrength);
        }
    }

    // ── Determinism: identical inputs ⇒ identical output ───────────────────────

    [Fact]
    public void IsDeterministic_RepeatedRuns_ProduceIdenticalOutput()
    {
        DimensionStrengths[] input =
        [
            Dim("d1", 5, 2),
            Dim("d2", 4, 4),
            Dim("d3", null, null),
            Dim("d4", 1, null),
        ];

        var first = ComparisonEngine.Compare(input);
        var second = ComparisonEngine.Compare(input);

        second.Should().BeEquivalentTo(first, o => o.WithStrictOrdering());
    }

    [Fact]
    public void PreservesStrengths_OnEachClassifiedDimension()
    {
        var result = ComparisonEngine.Compare([Dim("d", 5, 2)]);

        var only = result.Should().ContainSingle().Subject;
        only.YourStrength.Should().Be(5);
        only.TheirStrength.Should().Be(2);
    }
}
