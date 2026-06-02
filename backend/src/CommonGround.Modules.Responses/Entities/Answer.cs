namespace CommonGround.Modules.Responses.Entities;

public sealed class Answer
{
    public Guid Id { get; init; }
    public Guid ResponseSetId { get; init; }
    public Guid QuestionId { get; init; }
    public Guid PrimaryAnswerOptionId { get; init; }
    public Guid? SecondaryAnswerOptionId { get; init; }

    public ResponseSet ResponseSet { get; init; } = null!;
}
