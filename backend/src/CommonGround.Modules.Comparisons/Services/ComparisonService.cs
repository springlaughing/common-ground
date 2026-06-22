using CommonGround.Modules.Comparisons.Entities;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Comparisons.Services;

/// <summary>
/// Drives the comparison lifecycle. US1: creating an invite — a pending session, the Initiator
/// participant (with the inviter's self-label), and an active single-use invite, all persisted
/// together. Reaches other modules only through SharedKernel interfaces (Principle IV); never
/// touches raw answers (Principle I).
/// </summary>
internal sealed class ComparisonService : IComparisonService
{
    private readonly DbContext _db;
    private readonly InviteTokenService _inviteTokens;
    private readonly IResponseReader _responseReader;

    public ComparisonService(DbContext db, InviteTokenService inviteTokens, IResponseReader responseReader)
    {
        _db = db;
        _inviteTokens = inviteTokens;
        _responseReader = responseReader;
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
}
