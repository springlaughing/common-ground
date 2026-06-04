using CommonGround.Modules.Reporting.Entities;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Reporting;

public sealed class ScoringEngine
{
    private readonly IQuestionnaireReader _questionnaireReader;
    private readonly DbContext _db;

    public ScoringEngine(IQuestionnaireReader questionnaireReader, DbContext db)
    {
        _questionnaireReader = questionnaireReader;
        _db = db;
    }

    public async Task<IReadOnlyList<DimensionScore>> ScoreAsync(
        Guid responseSetId,
        Guid questionnaireVersionId,
        IReadOnlyList<ScoringInput> answers,
        CancellationToken ct = default)
    {
        // Collect all answer option IDs referenced in this submission
        var optionIds = new List<Guid>();
        foreach (var answer in answers)
        {
            optionIds.Add(answer.PrimaryAnswerOptionId);
            if (answer.SecondaryAnswerOptionId.HasValue)
                optionIds.Add(answer.SecondaryAnswerOptionId.Value);
        }

        var weights = await _questionnaireReader.GetDimensionWeightsForOptionsAsync(optionIds, ct);
        var weightsByOption = weights
            .GroupBy(w => w.AnswerOptionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<DimensionWeightDto>)g.ToList());

        // Accumulate: primary × 1.0, secondary × 0.5
        var rawScores = AccumulateRawScores(answers, weightsByOption);

        var maxScores = await _questionnaireReader.GetDimensionMaxScoresAsync(questionnaireVersionId, ct);

        var dimensionScores = new List<DimensionScore>(rawScores.Count);
        foreach (var (dimensionId, rawScore) in rawScores)
        {
            maxScores.TryGetValue(dimensionId, out var maxScore);
            var normScore = maxScore > 0m
                ? Math.Max(0m, Math.Min(1m, rawScore / maxScore))
                : 0m;

            var ds = new DimensionScore
            {
                Id = Guid.NewGuid(),
                ResponseSetId = responseSetId,
                DimensionId = dimensionId,
                RawScore = rawScore,
                NormalisedScore = normScore,
            };
            _db.Set<DimensionScore>().Add(ds);
            dimensionScores.Add(ds);
        }

        await _db.SaveChangesAsync(ct);
        return dimensionScores;
    }

    private static Dictionary<string, decimal> AccumulateRawScores(
        IReadOnlyList<ScoringInput> answers,
        IReadOnlyDictionary<Guid, IReadOnlyList<DimensionWeightDto>> weightsByOption)
    {
        var rawScores = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var answer in answers)
        {
            AddWeights(rawScores, weightsByOption, answer.PrimaryAnswerOptionId, 1.0m);
            if (answer.SecondaryAnswerOptionId.HasValue)
                AddWeights(rawScores, weightsByOption, answer.SecondaryAnswerOptionId.Value, 0.5m);
        }

        return rawScores;
    }

    private static void AddWeights(
        Dictionary<string, decimal> rawScores,
        IReadOnlyDictionary<Guid, IReadOnlyList<DimensionWeightDto>> weightsByOption,
        Guid optionId,
        decimal factor)
    {
        if (!weightsByOption.TryGetValue(optionId, out var weights))
            return;

        foreach (var w in weights)
        {
            rawScores.TryGetValue(w.DimensionId, out var cur);
            rawScores[w.DimensionId] = cur + w.Weight * factor;
        }
    }
}
