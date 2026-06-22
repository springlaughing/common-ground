using CommonGround.SharedKernel.Interfaces;

namespace CommonGround.Modules.Comparisons.Services;

/// <summary>
/// US4 — assembles the per-viewer comparison report by pairing the deterministic
/// <see cref="ComparisonEngine"/> classification with Reporting's localized per-dimension title and
/// text (reused from feature 002 — no new comparison content authored). Computed on read: nothing is
/// stored, so the report always reflects the current responses. Groups keep their canonical order;
/// within each group, differences come before similarities, and dimensions neither person shows are
/// dropped. The localized text follows Reporting's locale-first English fallback.
/// </summary>
public sealed class ComparisonAssembler
{
    private readonly IReportingService _reporting;

    public ComparisonAssembler(IReportingService reporting) => _reporting = reporting;

    public async Task<ComparisonReportDto> AssembleAsync(
        Guid viewerResponseSetId,
        Guid otherResponseSetId,
        string otherLabel,
        string locale,
        CancellationToken ct = default)
    {
        var viewer = await _reporting.GetComparisonSourceAsync(viewerResponseSetId, locale, ct);
        var other = await _reporting.GetComparisonSourceAsync(otherResponseSetId, locale, ct);

        // Both sources share the same versioned group/dimension structure, so match by id.
        var otherGroupsById = other.Groups.ToDictionary(g => g.Id, StringComparer.Ordinal);

        var groups = new List<ComparisonReportGroupDto>();
        foreach (var viewerGroup in viewer.Groups)
        {
            if (!otherGroupsById.TryGetValue(viewerGroup.Id, out var otherGroup))
                continue;

            var otherDimsById = otherGroup.Dimensions.ToDictionary(d => d.DimensionId, StringComparer.Ordinal);

            // Classify each dimension (differences first, then similarities; omitted dropped).
            var pairs = viewerGroup.Dimensions
                .Select(d => new DimensionStrengths(
                    d.DimensionId,
                    d.Strength,
                    otherDimsById.GetValueOrDefault(d.DimensionId)?.Strength))
                .ToList();
            var classified = ComparisonEngine.Compare(pairs);
            if (classified.Count == 0)
                continue;

            var viewerDimsById = viewerGroup.Dimensions.ToDictionary(d => d.DimensionId, StringComparer.Ordinal);

            var insights = classified.Select(c =>
            {
                var viewerDim = viewerDimsById[c.DimensionId];
                var otherDim = otherDimsById.GetValueOrDefault(c.DimensionId);
                return new ComparisonReportInsightDto(
                    c.DimensionId,
                    viewerDim.Title,
                    c.YourStrength,
                    c.TheirStrength,
                    viewerDim.Text,
                    otherDim?.Text,
                    c.Kind == DimensionComparisonKind.Similarity ? "similarity" : "difference");
            }).ToList();

            groups.Add(new ComparisonReportGroupDto(viewerGroup.Id, viewerGroup.Title, insights));
        }

        return new ComparisonReportDto(otherLabel, groups);
    }
}
