using System.Security.Claims;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly IResponseReader _responseReader;
    private readonly IReportingService _reportingService;

    public MeController(IResponseReader responseReader, IReportingService reportingService)
    {
        _responseReader = responseReader;
        _reportingService = reportingService;
    }

    // T033 — returns the reflection for the session identified by the cg_session
    // cookie. The cookie's `sub` claim is the ResponseSetId (JwtBearer maps it to
    // NameIdentifier by default; fall back to the raw claim if mapping is off).
    [HttpGet("reflection")]
    public async Task<IActionResult> GetReflection([FromQuery] string? locale, CancellationToken ct)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(subject, out var responseSetId))
            return Unauthorized();

        var responseSet = await _responseReader.GetByIdAsync(responseSetId, ct);
        if (responseSet is null || responseSet.IsDeleted)
            return NotFound();

        // US4 — a saved reflection renders in the viewer's current locale (English fallback).
        // No locale is stored on the response; it is re-assembled per request, so switching
        // language at view time re-renders the same insights/strengths in the new language.
        var reflection = await _reportingService.AssembleReflectionAsync(responseSetId, SupportedLocales.Resolve(locale), ct);
        return Ok(new MeReflectionResult(reflection, responseSet.HasAccessCode));
    }
}

public sealed record MeReflectionResult(ReflectionDto Reflection, bool AccessCodeAvailable);
