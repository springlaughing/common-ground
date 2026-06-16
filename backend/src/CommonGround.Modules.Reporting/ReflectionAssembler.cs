using CommonGround.Modules.Reporting.Entities;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Reporting;

public sealed class ReflectionAssembler : IReportingService
{
    private const decimal DisplayThreshold = 0.4m;

    private readonly DbContext _db;

    public ReflectionAssembler(DbContext db) => _db = db;

    public async Task<ReflectionDto> AssembleReflectionAsync(Guid responseSetId, string locale, CancellationToken ct = default)
    {
        var scores = await _db.Set<DimensionScore>()
            .AsNoTracking()
            .Where(s => s.ResponseSetId == responseSetId)
            .ToListAsync(ct);

        if (scores.Count == 0)
            return new ReflectionDto([]);

        var scoreByDimension = scores.ToDictionary(
            s => s.DimensionId,
            s => s.NormalisedScore,
            StringComparer.Ordinal);

        var qualifyingIds = scoreByDimension
            .Where(kvp => kvp.Value >= DisplayThreshold)
            .Select(kvp => kvp.Key)
            .ToList();

        if (qualifyingIds.Count == 0)
            return new ReflectionDto([]);

        // English text is canonical on the base entities (the field-level fallback);
        // a non-default locale overrides it where a translation row exists.
        var snippetText = await _db.Set<InsightSnippet>()
            .AsNoTracking()
            .Where(s => qualifyingIds.Contains(s.DimensionId))
            .ToDictionaryAsync(s => s.DimensionId, s => s.Text, StringComparer.Ordinal, ct);

        // Per-insight titles are locale-first (US3): one row per (dimension, locale),
        // English being the fallback. Loaded for every locale, not just non-default.
        var titleByDimension = await LoadDimensionTitlesAsync(locale, qualifyingIds, ct);

        var groups = await _db.Set<DimensionGroup>()
            .AsNoTracking()
            .Include(g => g.Memberships.OrderBy(m => m.OrderIndex))
            .OrderBy(g => g.OrderIndex)
            .ToListAsync(ct);

        var groupTitle = new Dictionary<Guid, string>();

        if (locale != SupportedLocales.Default)
        {
            await ApplyTranslationsAsync(locale, qualifyingIds, snippetText, groupTitle, ct);
        }

        var reflectionGroups = new List<ReflectionGroupDto>();
        foreach (var group in groups)
        {
            var insights = BuildInsights(group, scoreByDimension, snippetText, titleByDimension);
            if (insights.Count > 0)
                reflectionGroups.Add(new ReflectionGroupDto(
                    group.GroupId,
                    groupTitle.GetValueOrDefault(group.Id, group.Title),
                    insights));
        }

        return new ReflectionDto(reflectionGroups);
    }

    private async Task ApplyTranslationsAsync(
        string locale,
        List<string> qualifyingIds,
        Dictionary<string, string> snippetText,
        Dictionary<Guid, string> groupTitle,
        CancellationToken ct)
    {
        var translatedSnippets = await (
            from s in _db.Set<InsightSnippet>().AsNoTracking()
            join t in _db.Set<InsightSnippetTranslation>().AsNoTracking() on s.Id equals t.InsightSnippetId
            where t.Locale == locale && qualifyingIds.Contains(s.DimensionId)
            select new { s.DimensionId, t.Text })
            .ToListAsync(ct);
        foreach (var s in translatedSnippets)
            snippetText[s.DimensionId] = s.Text;

        var translatedTitles = await _db.Set<DimensionGroupTranslation>()
            .AsNoTracking()
            .Where(t => t.Locale == locale)
            .ToListAsync(ct);
        foreach (var t in translatedTitles)
            groupTitle[t.DimensionGroupId] = t.Title;
    }

    /// <summary>
    /// Loads per-dimension titles for the qualifying insights. Titles are locale-first:
    /// the English ("en") row is the fallback, overridden by the requested locale where
    /// a row exists. Returns a map keyed by dimension id.
    /// </summary>
    private async Task<Dictionary<string, string>> LoadDimensionTitlesAsync(
        string locale,
        List<string> qualifyingIds,
        CancellationToken ct)
    {
        var rows = await _db.Set<DimensionTitle>()
            .AsNoTracking()
            .Where(t => qualifyingIds.Contains(t.DimensionId)
                        && (t.Locale == SupportedLocales.Default || t.Locale == locale))
            .ToListAsync(ct);

        var byDimension = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var t in rows.Where(t => t.Locale == SupportedLocales.Default))
            byDimension[t.DimensionId] = t.Title;
        if (locale != SupportedLocales.Default)
            foreach (var t in rows.Where(t => t.Locale == locale))
                byDimension[t.DimensionId] = t.Title;
        return byDimension;
    }

    private static List<InsightDto> BuildInsights(
        DimensionGroup group,
        Dictionary<string, decimal> scoreByDimension,
        Dictionary<string, string> snippetText,
        Dictionary<string, string> titleByDimension) =>
        group.Memberships
            .Where(m => scoreByDimension.GetValueOrDefault(m.DimensionId) >= DisplayThreshold
                        && snippetText.ContainsKey(m.DimensionId))
            // Strongest first within each group. Ties fall back to the group-definition
            // order (OrderIndex), so ordering is deterministic and locale-invariant
            // (scores don't depend on locale). The strength dots already convey "more",
            // so this just surfaces the existing order.
            .OrderByDescending(m => scoreByDimension[m.DimensionId])
            .ThenBy(m => m.OrderIndex)
            .Select(m => new InsightDto(
                m.DimensionId,
                titleByDimension.GetValueOrDefault(m.DimensionId, string.Empty),
                snippetText[m.DimensionId],
                Math.Max(1, Math.Min(5, (int)Math.Ceiling(scoreByDimension[m.DimensionId] * 5m)))))
            .ToList();
}
