using System.Security.Claims;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api/comparisons")]
[Authorize]
public sealed class ComparisonsController : ControllerBase
{
    // Matches Invite.InviterLabel / ComparisonParticipant.DisplayLabel column length.
    private const int MaxLabelLength = 60;

    private readonly IComparisonService _comparisonService;
    private readonly IAuditLogger _auditLogger;

    public ComparisonsController(IComparisonService comparisonService, IAuditLogger auditLogger)
    {
        _comparisonService = comparisonService;
        _auditLogger = auditLogger;
    }

    // US1 (T015) — the inviter mints a single-use, time-limited invite for a new comparison.
    // Session-authenticated: the cg_session `sub` claim is the inviter's ResponseSetId.
    [HttpPost]
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
}

public sealed record CreateInviteRequest(string? InviterLabel);
public sealed record CreateInviteResponse(Guid ComparisonId, string InviteToken, DateTimeOffset ExpiresAt, string Status);
