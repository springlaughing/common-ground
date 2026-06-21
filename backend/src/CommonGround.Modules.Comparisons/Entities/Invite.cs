namespace CommonGround.Modules.Comparisons.Entities;

/// <summary>
/// A single-use, time-limited credential the inviter shares so one other person can join a
/// comparison. The plain token lives only in the link fragment (<c>/invite#TOKEN</c>); only its
/// HMAC-SHA256 hash is stored. Created together with its <see cref="ComparisonSession"/> and the
/// Initiator participant. Status transitions Active → Used (on a successful join) or → Expired.
/// </summary>
public sealed class Invite
{
    public Guid Id { get; init; }
    public Guid ComparisonSessionId { get; init; }
    public Guid InviterResponseSetId { get; init; }

    /// <summary>The inviter's self-label, shown to the invitee at consent. Bounded, required, non-verified.</summary>
    public string InviterLabel { get; init; } = string.Empty;

    /// <summary>HMAC-SHA256 of the invite token. The plain token is never stored.</summary>
    public string TokenHash { get; init; } = string.Empty;

    public InviteStatus Status { get; set; }

    /// <summary>Created + a fixed window (default 7 days). A short grace period past this still allows an in-progress join.</summary>
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public enum InviteStatus
{
    Active,
    Used,
    Expired,
}
