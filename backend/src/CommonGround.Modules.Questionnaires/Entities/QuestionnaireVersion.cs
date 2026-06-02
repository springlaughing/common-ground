namespace CommonGround.Modules.Questionnaires.Entities;

public sealed class QuestionnaireVersion
{
    public Guid Id { get; init; }
    public string VersionNumber { get; init; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    public ICollection<Question> Questions { get; init; } = [];
    public ICollection<DimensionMaxScore> DimensionMaxScores { get; init; } = [];
}
