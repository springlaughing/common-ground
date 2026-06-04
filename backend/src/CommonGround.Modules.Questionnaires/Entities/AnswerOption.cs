namespace CommonGround.Modules.Questionnaires.Entities;

public sealed class AnswerOption
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int OrderIndex { get; init; }

    public Question Question { get; init; } = null!;
    public ICollection<DimensionWeight> DimensionWeights { get; init; } = [];
}
