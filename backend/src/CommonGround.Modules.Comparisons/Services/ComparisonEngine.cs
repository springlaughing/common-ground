namespace CommonGround.Modules.Comparisons.Services;

/// <summary>How a single dimension reads across the two participants.</summary>
public enum DimensionComparisonKind
{
    Difference,
    Similarity,
}

/// <summary>Engine input for one dimension: each side's display strength, or <c>null</c> when below
/// the display threshold (no clear signal for that person).</summary>
public sealed record DimensionStrengths(string DimensionId, int? YourStrength, int? TheirStrength);

/// <summary>One classified dimension of the report, carrying both sides' strengths for display.</summary>
public sealed record DimensionComparison(string DimensionId, int? YourStrength, int? TheirStrength, DimensionComparisonKind Kind);

/// <summary>
/// The pure, deterministic pair-comparison core (Principle III). Given each shared dimension's
/// display strengths for two people, it classifies the dimension as a similarity or a difference and
/// drops the ones neither person shows. Differences come first, then similarities; within each, input
/// order is preserved. No I/O, no localization, no randomness — identical inputs always yield
/// identical output, and the result is symmetric per viewpoint (swapping the two people swaps each
/// pair of strengths but never changes a classification).
/// </summary>
public static class ComparisonEngine
{
    /// <summary>Both shown and within this absolute strength gap → aligned; a wider gap → a difference.</summary>
    public const int SimilarityGap = 1;

    public static IReadOnlyList<DimensionComparison> Compare(IReadOnlyList<DimensionStrengths> dimensions)
    {
        var classified = new List<DimensionComparison>(dimensions.Count);
        foreach (var dimension in dimensions)
        {
            var kind = Classify(dimension.YourStrength, dimension.TheirStrength);
            if (kind is null)
                continue; // both below threshold — neither has a clear signal, so it is omitted
            classified.Add(new DimensionComparison(
                dimension.DimensionId, dimension.YourStrength, dimension.TheirStrength, kind.Value));
        }

        // Differences first, then similarities; stable within each bucket (preserves input order).
        return
        [
            .. classified.Where(c => c.Kind == DimensionComparisonKind.Difference),
            .. classified.Where(c => c.Kind == DimensionComparisonKind.Similarity),
        ];
    }

    private static DimensionComparisonKind? Classify(int? yourStrength, int? theirStrength)
    {
        var youShow = yourStrength.HasValue;
        var theyShow = theirStrength.HasValue;

        // Neither person shows the dimension — no clear signal for either side.
        if (!youShow && !theyShow)
            return null;

        // Both show it: a small gap reads as aligned, a wider gap as a difference.
        if (youShow && theyShow)
            return Math.Abs(yourStrength!.Value - theirStrength!.Value) <= SimilarityGap
                ? DimensionComparisonKind.Similarity
                : DimensionComparisonKind.Difference;

        // Exactly one side shows it — a one-sided difference.
        return DimensionComparisonKind.Difference;
    }
}
