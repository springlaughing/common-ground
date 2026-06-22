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

    /// <summary>
    /// US2 — the public face of an invite for the consent screen, <b>without consuming it</b>: the
    /// inviter's self-label, the (lazily computed) status, and the version the invitee would answer.
    /// Returns <c>null</c> when no invite matches the token. Audits <c>comparison_invite_opened</c>.
    /// </summary>
    Task<InviteValidationResult?> ValidateInviteAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// US2 — reads an invite that is currently joinable (active, within the grace window), returning
    /// the comparison id and the version it is pinned to, <b>without consuming it</b>. Returns
    /// <c>null</c> when the token is unknown or the invite is no longer joinable — callers surface a
    /// single neutral error either way (no existence leak).
    /// </summary>
    Task<JoinableInvite?> GetJoinableInviteAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// US2 — consumes the invite single-use (<c>Active→Used</c>) and adds the Invitee participant
    /// (with their self-label) to the comparison. Re-checks joinability as the authority. Does NOT
    /// commit — the caller wraps this together with the invitee's response creation in one
    /// transaction. Returns the comparison id.
    /// </summary>
    Task<Guid> CompleteJoinAsync(string token, Guid inviteeResponseSetId, string inviteeLabel, CancellationToken ct = default);

    /// <summary>
    /// US4 — every comparison the viewer's response is part of, newest first, each with the other
    /// participant's label and the session status (pending until the invitee joins).
    /// </summary>
    Task<IReadOnlyList<ComparisonSummaryDto>> ListComparisonsAsync(Guid viewerResponseSetId, CancellationToken ct = default);

    /// <summary>
    /// US4 — the comparison report from the viewer's perspective ("you" = the viewer's response),
    /// computed on read in the requested locale. Returns the access state: a non-participant gets
    /// <see cref="ComparisonReportState.AccessDenied"/> (and an <c>access_denied</c> audit); an
    /// unfinished or unavailable comparison returns the matching marker state instead of a report.
    /// </summary>
    Task<ComparisonReportResult> GetReportAsync(Guid viewerResponseSetId, Guid comparisonId, string locale, CancellationToken ct = default);
}

/// <param name="ComparisonId">The comparison session id.</param>
/// <param name="OtherLabel">The other participant's self-label (empty while still pending).</param>
/// <param name="Status">"pending" · "complete" · "unavailable".</param>
public sealed record ComparisonSummaryDto(Guid ComparisonId, string OtherLabel, string Status, DateTimeOffset CreatedAt);

public enum ComparisonReportState
{
    Ready,
    Pending,
    Unavailable,
    AccessDenied,
    NotFound,
}

/// <param name="State">Whether a report is available, and if not, why.</param>
/// <param name="Report">The assembled report when <see cref="State"/> is <see cref="ComparisonReportState.Ready"/>; otherwise null.</param>
public sealed record ComparisonReportResult(ComparisonReportState State, ComparisonReportDto? Report);

/// <summary>The per-viewer comparison report — second person for "you", the other named by label.
/// No overall compatibility score and no raw answers (Principles I, II).</summary>
public sealed record ComparisonReportDto(string OtherLabel, IReadOnlyList<ComparisonReportGroupDto> Groups);

public sealed record ComparisonReportGroupDto(string Id, string Title, IReadOnlyList<ComparisonReportInsightDto> Insights);

/// <param name="YourStrength">The viewer's 1–5 strength, or null if below the display threshold.</param>
/// <param name="TheirStrength">The other participant's 1–5 strength, or null if below threshold.</param>
/// <param name="YourText">The viewer's localized insight text, omitted when below threshold.</param>
/// <param name="TheirText">The other's localized insight text, omitted when below threshold.</param>
/// <param name="Classification">"similarity" or "difference".</param>
public sealed record ComparisonReportInsightDto(
    string DimensionId,
    string Title,
    int? YourStrength,
    int? TheirStrength,
    string? YourText,
    string? TheirText,
    string Classification);

/// <param name="ComparisonId">The new comparison session's id (shown as pending on the inviter's /me).</param>
/// <param name="InviteToken">The plain, single-use token — the client builds <c>/invite#&lt;token&gt;</c>; never persisted plain.</param>
/// <param name="ExpiresAt">When the invite stops being valid (a short join grace period applies, US2).</param>
public sealed record CreateInviteResult(Guid ComparisonId, string InviteToken, DateTimeOffset ExpiresAt);

/// <param name="InviterLabel">The inviter's self-label, shown to the invitee at consent.</param>
/// <param name="Status">Lazily computed: <c>active</c> · <c>used</c> · <c>expired</c>.</param>
/// <param name="QuestionnaireVersion">The version number the invitee would answer.</param>
public sealed record InviteValidationResult(string InviterLabel, string Status, string QuestionnaireVersion);

/// <param name="ComparisonId">The comparison the invite belongs to.</param>
/// <param name="QuestionnaireVersionId">The version the invite is pinned to (for the same-version check).</param>
public sealed record JoinableInvite(Guid ComparisonId, Guid QuestionnaireVersionId);
