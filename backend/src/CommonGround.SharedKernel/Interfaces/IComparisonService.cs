namespace CommonGround.SharedKernel.Interfaces;

/// <summary>
/// Orchestrates the comparison lifecycle (invite → join → generate → view). Exposed through
/// SharedKernel so the Api host can drive it without referencing the Comparisons module's internals.
/// Grows per user story; US1 adds invite creation.
/// </summary>
public interface IComparisonService
{
    /// <summary>
    /// US1 — the inviter mints a single-use, time-limited invite for a new comparison. Creates a
    /// pending <c>ComparisonSession</c>, the Initiator participant (carrying the inviter's self-label),
    /// and an active invite. Returns the plain invite token (for the <c>/invite#TOKEN</c> link; never
    /// stored plain) and its expiry. The link never exposes the inviter's results.
    /// </summary>
    Task<CreateInviteResult> CreateInviteAsync(Guid inviterResponseSetId, string inviterLabel, CancellationToken ct = default);
}

/// <param name="ComparisonId">The new comparison session's id (shown as pending on the inviter's /me).</param>
/// <param name="InviteToken">The plain, single-use token — the client builds <c>/invite#&lt;token&gt;</c>; never persisted plain.</param>
/// <param name="ExpiresAt">When the invite stops being valid (a short join grace period applies, US2).</param>
public sealed record CreateInviteResult(Guid ComparisonId, string InviteToken, DateTimeOffset ExpiresAt);
