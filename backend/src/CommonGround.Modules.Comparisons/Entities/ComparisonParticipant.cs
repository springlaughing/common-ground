namespace CommonGround.Modules.Comparisons.Entities;

public sealed class ComparisonParticipant
{
    public Guid Id { get; init; }
    public Guid ComparisonSessionId { get; init; }
    public Guid ResponseSetId { get; init; }
    public ParticipantRole Role { get; init; }

    /// <summary>This participant's self-label, shown to the *other* participant. The Initiator's is
    /// copied from <see cref="Invite.InviterLabel"/>; the Invitee's is captured at join. Bounded, required.</summary>
    public string DisplayLabel { get; init; } = string.Empty;

    public DateTimeOffset JoinedAt { get; init; }

    public ComparisonSession ComparisonSession { get; init; } = null!;
}

public enum ParticipantRole
{
    Initiator,
    Invitee,
}
