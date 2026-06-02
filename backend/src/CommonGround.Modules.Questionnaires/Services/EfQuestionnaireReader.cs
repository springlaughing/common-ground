using CommonGround.Modules.Questionnaires.Entities;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Questionnaires.Services;

internal sealed class EfQuestionnaireReader : IQuestionnaireReader
{
    private readonly DbContext _db;

    public EfQuestionnaireReader(DbContext db) => _db = db;

    public async Task<ActiveQuestionnaireDto?> GetActiveVersionAsync(CancellationToken ct = default)
    {
        var version = await _db.Set<QuestionnaireVersion>()
            .AsNoTracking()
            .Include(v => v.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.AnswerOptions.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(v => v.IsActive, ct);

        if (version is null)
            return null;

        return new ActiveQuestionnaireDto(
            version.Id,
            version.VersionNumber,
            version.Questions
                .Select(q => new QuestionDto(
                    q.Id,
                    q.Text,
                    q.SectionIndex,
                    q.OrderIndex,
                    q.AnswerOptions
                        .Select(a => new AnswerOptionDto(a.Id, a.Text, a.OrderIndex))
                        .ToList()))
                .ToList());
    }

    public async Task<IReadOnlyList<DimensionWeightDto>> GetDimensionWeightsForOptionsAsync(
        IEnumerable<Guid> answerOptionIds,
        CancellationToken ct = default)
    {
        var ids = answerOptionIds.ToList();
        return await _db.Set<DimensionWeight>()
            .AsNoTracking()
            .Where(w => ids.Contains(w.AnswerOptionId))
            .Select(w => new DimensionWeightDto(w.AnswerOptionId, w.DimensionId, w.Weight))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetDimensionMaxScoresAsync(
        Guid questionnaireVersionId,
        CancellationToken ct = default)
    {
        return await _db.Set<DimensionMaxScore>()
            .AsNoTracking()
            .Where(d => d.QuestionnaireVersionId == questionnaireVersionId)
            .ToDictionaryAsync(d => d.DimensionId, d => d.MaxScore, ct);
    }
}
