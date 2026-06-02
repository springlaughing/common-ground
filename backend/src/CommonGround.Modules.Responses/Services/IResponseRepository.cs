namespace CommonGround.Modules.Responses.Services;

public sealed record AnswerInput(
    Guid QuestionId,
    Guid PrimaryAnswerOptionId,
    Guid? SecondaryAnswerOptionId);

public interface IResponseRepository
{
    Task<Entities.ResponseSet> CreateAsync(
        Guid questionnaireVersionId,
        string privateResultTokenHash,
        string accessCodeHash,
        IReadOnlyList<AnswerInput> answers,
        CancellationToken ct = default);
}
