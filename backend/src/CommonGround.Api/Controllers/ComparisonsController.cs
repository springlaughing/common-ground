using System.Security.Claims;
using CommonGround.Api.Application;
using CommonGround.Modules.Responses.Services;
using CommonGround.Api.Auth;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ComparisonsController : ControllerBase
{
    // Matches Invite.InviterLabel / ComparisonParticipant.DisplayLabel column length.
    private const int MaxLabelLength = 60;

    private readonly IComparisonService _comparisonService;
    private readonly InviteJoinService _joinService;
    private readonly SessionTokenIssuer _tokenIssuer;
    private readonly IAuditLogger _auditLogger;

    public ComparisonsController(
        IComparisonService comparisonService,
        InviteJoinService joinService,
        SessionTokenIssuer tokenIssuer,
        IAuditLogger auditLogger)
    {
        _comparisonService = comparisonService;
        _joinService = joinService;
        _tokenIssuer = tokenIssuer;
        _auditLogger = auditLogger;
    }

    // US1 (T015) — the inviter mints a single-use, time-limited invite for a new comparison.
    // Session-authenticated: the cg_session `sub` claim is the inviter's ResponseSetId.
    [HttpPost("comparisons")]
    [Authorize]
    [EnableRateLimiting("PostPolicy")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateInvite(CreateInviteRequest request, CancellationToken ct)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(subject, out var responseSetId))
            return Unauthorized();

        var label = request.InviterLabel?.Trim();
        if (string.IsNullOrEmpty(label))
            return BadRequest(new ValidationError("invalid_label", "A label is required."));
        if (label.Length > MaxLabelLength)
            return BadRequest(new ValidationError("invalid_label", $"The label must be at most {MaxLabelLength} characters."));

        CreateInviteResult result;
        try
        {
            result = await _comparisonService.CreateInviteAsync(responseSetId, label, ct);
        }
        catch (DomainException)
        {
            // The session pointed at a response that no longer exists (e.g. deleted).
            return Unauthorized();
        }

        await _auditLogger.LogAsync("comparison_invite_created", responseSetId, result.ComparisonId, ct: ct);

        // A freshly created session has no invitee yet, so it is always pending.
        return StatusCode(StatusCodes.Status201Created, new CreateInviteResponse(
            result.ComparisonId,
            result.InviteToken,
            result.ExpiresAt,
            "pending"));
    }

    // US2 (T024) — the public face of an invite for the consent screen. No session: reached straight
    // from the invite link. Never consumes the invite; errors are neutral (no existence leak).
    [HttpPost("invite/validate")]
    [EnableRateLimiting("PostPolicy")]
    [Consumes("application/json")]
    public async Task<IActionResult> ValidateInvite(ValidateInviteRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return NotFound(InviteUnavailable);

        var result = await _comparisonService.ValidateInviteAsync(request.Token, ct);
        if (result is null)
            return NotFound(InviteUnavailable);

        // Found but no longer usable (used/expired) — Gone, still neutral.
        if (result.Status != "active")
            return StatusCode(StatusCodes.Status410Gone, InviteUnavailable);

        return Ok(result);
    }

    // US2 (T024) — the invitee has consented and completed the questionnaire. Consumes the invite
    // single-use, creates the invitee's own response, starts their session, returns their credentials.
    [HttpPost("invite/join")]
    [EnableRateLimiting("PostPolicy")]
    [Consumes("application/json")]
    public async Task<IActionResult> JoinInvite(JoinInviteRequest request, CancellationToken ct)
    {
        var answers = (request.Answers ?? [])
            .Select(a => new AnswerInput(a.QuestionId, a.PrimaryAnswerOptionId, a.SecondaryAnswerOptionId))
            .ToList();

        var command = new JoinInviteCommand(request.Token ?? string.Empty, request.Consent, request.InviteeLabel, answers);
        var result = await _joinService.JoinAsync(command, ct);

        if (!result.IsSuccess)
            return JoinError(result.ErrorCode!, result.ErrorMessage!);

        var joined = result.Value!;

        // Start the invitee's own session (same cookie contract as /session/start).
        var (jwt, maxAge) = _tokenIssuer.Issue(joined.InviteeResponseSetId);
        Response.Cookies.Append("cg_session", jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = maxAge,
            Path = "/",
        });

        return StatusCode(StatusCodes.Status201Created, new JoinInviteResponse(
            joined.PrivateResultLink,
            joined.AccessCode,
            joined.ComparisonId));
    }

    // "Used/expired (outside grace)" → 409; everything else (consent, label, version, answers) → 400.
    private IActionResult JoinError(string code, string message) =>
        code == "invite_not_joinable"
            ? Conflict(new ValidationError(code, message))
            : BadRequest(new ValidationError(code, message));

    private static ValidationError InviteUnavailable =>
        new("invite_unavailable", "This invite link is not valid, has already been used, or has expired.");
}

public sealed record CreateInviteRequest(string? InviterLabel);
public sealed record CreateInviteResponse(Guid ComparisonId, string InviteToken, DateTimeOffset ExpiresAt, string Status);
public sealed record ValidateInviteRequest(string? Token);
public sealed record JoinInviteRequest(string? Token, bool Consent, string? InviteeLabel, IReadOnlyList<AnswerRequest>? Answers);
public sealed record JoinInviteResponse(string PrivateResultLink, string AccessCode, Guid ComparisonId);
