namespace CommonGround.SharedKernel.Interfaces;

public interface IQuestionnaireReader
{
    Task<ActiveQuestionnaireDto?> GetActiveVersionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DimensionWeightDto>> GetDimensionWeightsForOptionsAsync(IEnumerable<Guid> answerOptionIds, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, decimal>> GetDimensionMaxScoresAsync(Guid questionnaireVersionId, CancellationToken ct = default);
}

public record ActiveQuestionnaireDto(
    Guid Id,
    string VersionNumber,
    IReadOnlyList<QuestionDto> Questions);

public record QuestionDto(
    Guid Id,
    string Text,
    int SectionIndex,
    int OrderIndex,
    IReadOnlyList<AnswerOptionDto> AnswerOptions);

public record AnswerOptionDto(
    Guid Id,
    string Text,
    int OrderIndex);

public record DimensionWeightDto(
    Guid AnswerOptionId,
    string DimensionId,
    int Weight);
