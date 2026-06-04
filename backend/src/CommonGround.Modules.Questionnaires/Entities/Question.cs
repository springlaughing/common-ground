namespace CommonGround.Modules.Questionnaires.Entities;

public sealed class Question
{
    public Guid Id { get; init; }
    public Guid QuestionnaireVersionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int SectionIndex { get; init; }
    public int OrderIndex { get; init; }

    public QuestionnaireVersion QuestionnaireVersion { get; init; } = null!;
    public ICollection<AnswerOption> AnswerOptions { get; init; } = [];
}
