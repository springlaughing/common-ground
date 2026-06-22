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
    private readonly IComparisonService _comparisonService;

    public MeController(IResponseReader responseReader, IReportingService reportingService, IComparisonService comparisonService)
    {
        _responseReader = responseReader;
        _reportingService = reportingService;
        _comparisonService = comparisonService;
    }

    // T033 — returns the reflection for the session identified by the cg_session
    // cookie. The cookie's `sub` claim is the ResponseSetId (JwtBearer maps it to
    // NameIdentifier by default; fall back to the raw claim if mapping is off).
    [HttpGet("reflection")]
    public async Task<IActionResult> GetReflection([FromQuery] string? locale, CancellationToken ct)
    {
        if (!TryGetResponseSetId(out var responseSetId))
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

    // US4 (T037) — lists every comparison the caller's response is part of (the /me hub).
    [HttpGet("comparisons")]
    public async Task<IActionResult> ListComparisons(CancellationToken ct)
    {
        if (!TryGetResponseSetId(out var responseSetId))
            return Unauthorized();

        var comparisons = await _comparisonService.ListComparisonsAsync(responseSetId, ct);
        return Ok(new ListComparisonsResult(comparisons));
    }

    // US4 (T037) — one comparison report from the caller's perspective, in the requested locale.
    [HttpGet("comparisons/{id:guid}")]
    public async Task<IActionResult> GetComparison(Guid id, [FromQuery] string? locale, CancellationToken ct)
    {
        if (!TryGetResponseSetId(out var responseSetId))
            return Unauthorized();

        var result = await _comparisonService.GetReportAsync(responseSetId, id, SupportedLocales.Resolve(locale), ct);
        return result.State switch
        {
            ComparisonReportState.Ready => Ok(result.Report),
            ComparisonReportState.Pending => Ok(new ComparisonStateResult("pending")),
            ComparisonReportState.Unavailable => Ok(new ComparisonStateResult("unavailable")),
            ComparisonReportState.AccessDenied => StatusCode(StatusCodes.Status403Forbidden, new ComparisonStateResult("access_denied")),
            _ => NotFound(),
        };
    }

    // The cg_session `sub` claim is the caller's ResponseSetId (JwtBearer maps it to
    // NameIdentifier by default; fall back to the raw claim if mapping is off).
    private bool TryGetResponseSetId(out Guid responseSetId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(subject, out responseSetId);
    }
}

public sealed record MeReflectionResult(ReflectionDto Reflection, bool AccessCodeAvailable);
public sealed record ListComparisonsResult(IReadOnlyList<ComparisonSummaryDto> Comparisons);
public sealed record ComparisonStateResult(string State);
