namespace CommonGround.Modules.Responses.Entities;

public sealed class ResponseSet
{
    public Guid Id { get; init; }
    public Guid QuestionnaireVersionId { get; init; }
    public string PrivateResultTokenHash { get; init; } = string.Empty;
    public string AccessCodeHash { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Answer> Answers { get; init; } = [];
}
