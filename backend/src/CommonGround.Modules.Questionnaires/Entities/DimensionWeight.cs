namespace CommonGround.Modules.Questionnaires.Entities;

public sealed class DimensionWeight
{
    public Guid Id { get; init; }
    public Guid AnswerOptionId { get; init; }
    public string DimensionId { get; init; } = string.Empty;
    public int Weight { get; init; }

    public AnswerOption AnswerOption { get; init; } = null!;
}
