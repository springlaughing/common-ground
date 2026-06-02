namespace CommonGround.Modules.Comparisons.Entities;

public sealed class ComparisonParticipant
{
    public Guid Id { get; init; }
    public Guid ComparisonSessionId { get; init; }
    public Guid ResponseSetId { get; init; }
    public ParticipantRole Role { get; init; }
    public DateTimeOffset JoinedAt { get; init; }

    public ComparisonSession ComparisonSession { get; init; } = null!;
}

public enum ParticipantRole
{
    Initiator,
    Invitee,
}
