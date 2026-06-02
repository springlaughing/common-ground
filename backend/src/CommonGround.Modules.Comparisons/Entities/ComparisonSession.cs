namespace CommonGround.Modules.Comparisons.Entities;

public sealed class ComparisonSession
{
    public Guid Id { get; init; }
    public Guid QuestionnaireVersionId { get; init; }
    public ComparisonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    public ICollection<ComparisonParticipant> Participants { get; init; } = [];
}

public enum ComparisonStatus
{
    Pending,
    Complete,
    Unavailable,
}
