using CommonGround.Modules.Reporting.Entities;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Reporting;

public sealed class ReflectionAssembler : IReportingService
{
    private const decimal DisplayThreshold = 0.4m;

    private readonly DbContext _db;

    public ReflectionAssembler(DbContext db) => _db = db;

    public async Task<ReflectionDto> AssembleReflectionAsync(Guid responseSetId, CancellationToken ct = default)
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

        var snippetText = await _db.Set<InsightSnippet>()
            .AsNoTracking()
            .Where(s => qualifyingIds.Contains(s.DimensionId))
            .ToDictionaryAsync(s => s.DimensionId, s => s.Text, StringComparer.Ordinal, ct);

        var groups = await _db.Set<DimensionGroup>()
            .AsNoTracking()
            .Include(g => g.Memberships.OrderBy(m => m.OrderIndex))
            .OrderBy(g => g.OrderIndex)
            .ToListAsync(ct);

        var reflectionGroups = new List<ReflectionGroupDto>();
        foreach (var group in groups)
        {
            var insights = new List<InsightDto>();
            foreach (var membership in group.Memberships)
            {
                if (!scoreByDimension.TryGetValue(membership.DimensionId, out var normScore))
                    continue;
                if (normScore < DisplayThreshold)
                    continue;
                if (!snippetText.TryGetValue(membership.DimensionId, out var text))
                    continue;

                var strength = Math.Max(1, Math.Min(5, (int)Math.Ceiling(normScore * 5m)));
                insights.Add(new InsightDto(membership.DimensionId, text, strength));
            }

            if (insights.Count > 0)
                reflectionGroups.Add(new ReflectionGroupDto(group.GroupId, group.Title, insights));
        }

        return new ReflectionDto(reflectionGroups);
    }
}
