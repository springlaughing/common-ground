using CommonGround.Modules.Questionnaires.Entities;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Questionnaires.Services;

internal sealed class EfQuestionnaireReader : IQuestionnaireReader
{
    private readonly DbContext _db;

    public EfQuestionnaireReader(DbContext db) => _db = db;

    public async Task<ActiveQuestionnaireDto?> GetActiveVersionAsync(string locale, CancellationToken ct = default)
    {
        var version = await _db.Set<QuestionnaireVersion>()
            .AsNoTracking()
            .Include(v => v.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.AnswerOptions.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(v => v.IsActive, ct);

        if (version is null)
            return null;

        // English text is canonical on the base columns and is the field-level fallback;
        // only a non-default locale needs a translation lookup.
        var (questionText, optionText) = locale == SupportedLocales.Default
            ? (new Dictionary<Guid, string>(), new Dictionary<Guid, string>())
            : await LoadTranslationsAsync(version, locale, ct);

        return new ActiveQuestionnaireDto(
            version.Id,
            version.VersionNumber,
            version.Questions
                .Select(q => new QuestionDto(
                    q.Id,
                    questionText.GetValueOrDefault(q.Id, q.Text),
                    q.SectionIndex,
                    q.OrderIndex,
                    q.AnswerOptions
                        .Select(a => new AnswerOptionDto(a.Id, optionText.GetValueOrDefault(a.Id, a.Text), a.OrderIndex))
                        .ToList()))
                .ToList());
    }

    private async Task<(Dictionary<Guid, string> Questions, Dictionary<Guid, string> Options)> LoadTranslationsAsync(
        QuestionnaireVersion version, string locale, CancellationToken ct)
    {
        var questionIds = version.Questions.Select(q => q.Id).ToList();
        var optionIds = version.Questions.SelectMany(q => q.AnswerOptions.Select(a => a.Id)).ToList();

        var questionText = await _db.Set<QuestionTranslation>()
            .AsNoTracking()
            .Where(t => t.Locale == locale && questionIds.Contains(t.QuestionId))
            .ToDictionaryAsync(t => t.QuestionId, t => t.Text, ct);

        var optionText = await _db.Set<AnswerOptionTranslation>()
            .AsNoTracking()
            .Where(t => t.Locale == locale && optionIds.Contains(t.AnswerOptionId))
            .ToDictionaryAsync(t => t.AnswerOptionId, t => t.Text, ct);

        return (questionText, optionText);
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
