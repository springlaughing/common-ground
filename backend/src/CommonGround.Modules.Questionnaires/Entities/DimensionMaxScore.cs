namespace CommonGround.Modules.Questionnaires.Entities;

public sealed class DimensionMaxScore
{
    public Guid Id { get; init; }
    public Guid QuestionnaireVersionId { get; init; }
    public string DimensionId { get; init; } = string.Empty;
    public decimal MaxScore { get; init; }

    public QuestionnaireVersion QuestionnaireVersion { get; init; } = null!;
}
