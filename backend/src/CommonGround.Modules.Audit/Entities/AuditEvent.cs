namespace CommonGround.Modules.Audit.Entities;

public sealed class AuditEvent
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public Guid? ResponseSetId { get; init; }
    public Guid? ComparisonSessionId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string? Metadata { get; init; }
}
