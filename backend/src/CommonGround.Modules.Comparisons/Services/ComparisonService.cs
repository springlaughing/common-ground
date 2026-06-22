using CommonGround.Modules.Comparisons.Entities;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Comparisons.Services;

/// <summary>
/// Drives the comparison lifecycle. US1: creating an invite. US2: validating an invite (no consume)
/// and consuming it single-use as the invitee joins. Reaches other modules only through SharedKernel
/// interfaces (Principle IV); never touches raw answers (Principle I).
/// </summary>
internal sealed class ComparisonService : IComparisonService
{
    private readonly DbContext _db;
    private readonly InviteTokenService _inviteTokens;
    private readonly IResponseReader _responseReader;
    private readonly IQuestionnaireReader _questionnaireReader;
    private readonly IAuditLogger _auditLogger;

    public ComparisonService(
        DbContext db,
        InviteTokenService inviteTokens,
        IResponseReader responseReader,
        IQuestionnaireReader questionnaireReader,
        IAuditLogger auditLogger)
    {
        _db = db;
        _inviteTokens = inviteTokens;
        _responseReader = responseReader;
        _questionnaireReader = questionnaireReader;
        _auditLogger = auditLogger;
    }

    public async Task<CreateInviteResult> CreateInviteAsync(Guid inviterResponseSetId, string inviterLabel, CancellationToken ct = default)
    {
        // The caller is session-authenticated, so their response should exist; guard anyway and
        // pin the comparison to the inviter's questionnaire version (same-version rule, US3).
        var inviter = await _responseReader.GetByIdAsync(inviterResponseSetId, ct);
        if (inviter is null || inviter.IsDeleted)
            throw new DomainException("inviter_response_not_found", "The inviter's response no longer exists.");

        var issued = _inviteTokens.Issue();
        var sessionId = Guid.NewGuid();

        var session = new ComparisonSession
        {
            Id = sessionId,
            QuestionnaireVersionId = inviter.QuestionnaireVersionId,
            Status = ComparisonStatus.Pending,
            CreatedAt = issued.CreatedAt,
            Participants =
            [
                new ComparisonParticipant
                {
                    Id = Guid.NewGuid(),
                    ComparisonSessionId = sessionId,
                    ResponseSetId = inviterResponseSetId,
                    Role = ParticipantRole.Initiator,
                    DisplayLabel = inviterLabel,
                    JoinedAt = issued.CreatedAt,
                },
            ],
        };

        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ComparisonSessionId = sessionId,
            InviterResponseSetId = inviterResponseSetId,
            InviterLabel = inviterLabel,
            TokenHash = issued.TokenHash,
            Status = InviteStatus.Active,
            CreatedAt = issued.CreatedAt,
            ExpiresAt = issued.ExpiresAt,
        };

        _db.Set<ComparisonSession>().Add(session);
        _db.Set<Invite>().Add(invite);
        await _db.SaveChangesAsync(ct);

        return new CreateInviteResult(sessionId, issued.PlainToken, issued.ExpiresAt);
    }

    public async Task<InviteValidationResult?> ValidateInviteAsync(string token, CancellationToken ct = default)
    {
        var invite = await _db.Set<Invite>()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.TokenHash == _inviteTokens.Hash(token), ct);
        if (invite is null)
            return null;

        // Record that the invite link was opened (no sensitive content; FR-006: the inviter is
        // never told who/when). Fires whatever the status, since the open attempt still happened.
        await _auditLogger.LogAsync("comparison_invite_opened", comparisonSessionId: invite.ComparisonSessionId, ct: ct);

        // The version the invitee would answer is the active one; the same-version rule is enforced
        // at join. Validation only needs the number for display.
        var active = await _questionnaireReader.GetActiveVersionAsync(SupportedLocales.Default, ct);

        return new InviteValidationResult(invite.InviterLabel, DisplayStatus(invite), active?.VersionNumber ?? string.Empty);
    }

    public async Task<JoinableInvite?> GetJoinableInviteAsync(string token, CancellationToken ct = default)
    {
        var invite = await _db.Set<Invite>()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.TokenHash == _inviteTokens.Hash(token), ct);

        if (invite is null || !InviteJoinRules.IsJoinable(invite.Status, invite.ExpiresAt, DateTimeOffset.UtcNow))
            return null;

        // The comparison's questionnaire version (pinned at invite creation) lives on the session.
        var session = await _db.Set<ComparisonSession>()
            .AsNoTracking()
            .FirstAsync(s => s.Id == invite.ComparisonSessionId, ct);

        return new JoinableInvite(invite.ComparisonSessionId, session.QuestionnaireVersionId);
    }

    public async Task<Guid> CompleteJoinAsync(string token, Guid inviteeResponseSetId, string inviteeLabel, CancellationToken ct = default)
    {
        // Tracked load so the consume + participant insert persist together (the caller's
        // transaction also wraps the invitee's response creation).
        var invite = await _db.Set<Invite>()
            .FirstOrDefaultAsync(i => i.TokenHash == _inviteTokens.Hash(token), ct);

        // Authoritative re-check: guards a race between GetJoinableInvite and here (single-use).
        if (invite is null || !InviteJoinRules.IsJoinable(invite.Status, invite.ExpiresAt, DateTimeOffset.UtcNow))
            throw new DomainException("invite_not_joinable", "This invite can no longer be used.");

        invite.Status = InviteStatus.Used;

        _db.Set<ComparisonParticipant>().Add(new ComparisonParticipant
        {
            Id = Guid.NewGuid(),
            ComparisonSessionId = invite.ComparisonSessionId,
            ResponseSetId = inviteeResponseSetId,
            Role = ParticipantRole.Invitee,
            DisplayLabel = inviteeLabel,
            JoinedAt = DateTimeOffset.UtcNow,
        });

        // US3 — the invitee's join completes the pair, so the comparison generates automatically.
        await GenerateIfReadyAsync(invite.ComparisonSessionId, inviteeResponseSetId, ct);

        await _db.SaveChangesAsync(ct);
        return invite.ComparisonSessionId;
    }

    // US3 — generation is compute-on-read, so "generating" is just marking the session Complete once
    // both responses exist on the same version (the report itself is assembled on read in US4). The
    // same-version assertion is defensive: join already enforces it, but it guards future paths
    // (e.g. access-code reuse in feature 004). Audits comparison_generated exactly once, on the
    // Pending→Complete transition.
    private async Task GenerateIfReadyAsync(Guid sessionId, Guid inviteeResponseSetId, CancellationToken ct)
    {
        var session = await _db.Set<ComparisonSession>().FirstAsync(s => s.Id == sessionId, ct);
        if (session.Status != ComparisonStatus.Pending)
            return;

        var inviteeResponse = await _responseReader.GetByIdAsync(inviteeResponseSetId, ct);
        if (inviteeResponse is null || inviteeResponse.QuestionnaireVersionId != session.QuestionnaireVersionId)
            return; // cross-version (shouldn't reach here in the happy path) — leave Pending, don't generate

        session.Status = ComparisonStatus.Complete;
        await _auditLogger.LogAsync("comparison_generated", comparisonSessionId: session.Id, ct: ct);
    }

    // Status is computed lazily — an Active invite past its window reads as expired without a
    // background sweep flipping it. (Consumption to Used is the only persisted transition here.)
    private static string DisplayStatus(Invite invite)
    {
        if (invite.Status == InviteStatus.Used)
            return "used";
        if (DateTimeOffset.UtcNow > invite.ExpiresAt)
            return "expired";
        return "active";
    }
}
