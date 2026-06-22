using CommonGround.Modules.Responses.Services;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Api.Application;

/// <summary>
/// Orchestrates the invitee join (US2) across modules — the only layer allowed to, being the
/// composition root. Wraps the whole thing in one transaction so the invitee's scored response, the
/// single-use invite consumption, and the Invitee participant either all land or none do: a
/// single-use credential and a partial failure must never coexist.
/// </summary>
public sealed class InviteJoinService
{
    // Matches the invite/participant label column length.
    private const int MaxLabelLength = 60;

    private readonly DbContext _db;
    private readonly ResponseSubmissionService _submission;
    private readonly IComparisonService _comparisons;
    private readonly IQuestionnaireReader _questionnaireReader;
    private readonly IAuditLogger _auditLogger;

    public InviteJoinService(
        DbContext db,
        ResponseSubmissionService submission,
        IComparisonService comparisons,
        IQuestionnaireReader questionnaireReader,
        IAuditLogger auditLogger)
    {
        _db = db;
        _submission = submission;
        _comparisons = comparisons;
        _questionnaireReader = questionnaireReader;
        _auditLogger = auditLogger;
    }

    public async Task<Result<JoinedComparison>> JoinAsync(JoinInviteCommand command, CancellationToken ct = default)
    {
        // Consent gate first (Principle V): nothing is created without explicit consent.
        if (!command.Consent)
            return Result.Failure<JoinedComparison>("consent_required", "Consent is required to join the comparison.");

        var label = command.InviteeLabel?.Trim();
        if (string.IsNullOrEmpty(label) || label.Length > MaxLabelLength)
            return Result.Failure<JoinedComparison>("invalid_label", "A label is required (max 60 characters).");

        // One unit of work for response + consume + participant: all-or-nothing. Disposing without
        // a commit (any early return / throw below) rolls everything back.
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var joinable = await _comparisons.GetJoinableInviteAsync(command.Token, ct);
        if (joinable is null)
            return Result.Failure<JoinedComparison>("invite_not_joinable", "This invite can no longer be used.");

        var active = await _questionnaireReader.GetActiveVersionAsync(SupportedLocales.Default, ct);
        if (active is null)
            return Result.Failure<JoinedComparison>("no_active_questionnaire", "No active questionnaire is available.");
        if (active.Id != joinable.QuestionnaireVersionId)
            return Result.Failure<JoinedComparison>("version_mismatch", "This invite is for a different questionnaire version.");

        var submitResult = await _submission.SubmitAsync(command.Answers, ct);
        if (!submitResult.IsSuccess)
            return Result.Failure<JoinedComparison>(submitResult.ErrorCode!, submitResult.ErrorMessage!);
        var submitted = submitResult.Value!;

        Guid comparisonId;
        try
        {
            comparisonId = await _comparisons.CompleteJoinAsync(command.Token, submitted.ResponseSetId, label, ct);
        }
        catch (DomainException ex)
        {
            // Lost a race for a single-use invite — roll back (incl. the just-created response).
            return Result.Failure<JoinedComparison>(ex.ErrorCode, ex.Message);
        }

        // Records consent + the join (Principle V), inside the transaction so it can't outlive a rollback.
        await _auditLogger.LogAsync("comparison_joined", submitted.ResponseSetId, comparisonId, ct: ct);

        await transaction.CommitAsync(ct);

        return Result.Success(new JoinedComparison(
            submitted.ResponseSetId,
            $"/me#{submitted.PlainToken}",
            submitted.PlainAccessCode,
            comparisonId));
    }
}

/// <param name="Token">The plain invite token (read from the link fragment client-side).</param>
/// <param name="Consent">Must be true — explicit consent (Principle V).</param>
/// <param name="InviteeLabel">The invitee's self-label, shown to the inviter.</param>
/// <param name="Answers">The invitee's questionnaire answers.</param>
public sealed record JoinInviteCommand(string Token, bool Consent, string? InviteeLabel, IReadOnlyList<AnswerInput> Answers);

/// <param name="InviteeResponseSetId">The invitee's own new response (used to start their session).</param>
public sealed record JoinedComparison(Guid InviteeResponseSetId, string PrivateResultLink, string AccessCode, Guid ComparisonId);
