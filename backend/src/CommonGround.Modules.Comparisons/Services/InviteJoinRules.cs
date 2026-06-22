using CommonGround.Modules.Comparisons.Entities;

namespace CommonGround.Modules.Comparisons.Services;

/// <summary>
/// Pure invite-join rules (Principle I + VI): single-use and time-limited, with a short grace
/// period so a join already in progress when the invite expires can still complete. Kept free of
/// I/O so the lifecycle is exhaustively unit-testable.
/// </summary>
public static class InviteJoinRules
{
    /// <summary>How long past <see cref="Invite.ExpiresAt"/> an in-progress join is still admitted.</summary>
    public static readonly TimeSpan JoinGracePeriod = TimeSpan.FromHours(1);

    /// <summary>
    /// An invite may be joined only while it is still <see cref="InviteStatus.Active"/> (single-use:
    /// a consumed <see cref="InviteStatus.Used"/> invite never re-opens) and within its validity
    /// window plus the grace period.
    /// </summary>
    public static bool IsJoinable(InviteStatus status, DateTimeOffset expiresAt, DateTimeOffset now)
        => status == InviteStatus.Active && now <= expiresAt + JoinGracePeriod;
}
