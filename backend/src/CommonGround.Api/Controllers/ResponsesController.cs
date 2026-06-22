using CommonGround.Api.Application;
using CommonGround.Modules.Responses.Services;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api/responses")]
public sealed class ResponsesController : ControllerBase
{
    private readonly ResponseSubmissionService _submission;
    private readonly IReportingService _reportingService;
    private readonly IAuditLogger _auditLogger;

    public ResponsesController(
        ResponseSubmissionService submission,
        IReportingService reportingService,
        IAuditLogger auditLogger)
    {
        _submission = submission;
        _reportingService = reportingService;
        _auditLogger = auditLogger;
    }

    [HttpPost]
    [EnableRateLimiting("PostPolicy")]
    [Consumes("application/json")]
    public async Task<IActionResult> Submit(SubmitResponseRequest request, [FromQuery] string? locale, CancellationToken ct)
    {
        var answers = request.Answers
            .Select(a => new AnswerInput(a.QuestionId, a.PrimaryAnswerOptionId, a.SecondaryAnswerOptionId))
            .ToList();

        var result = await _submission.SubmitAsync(answers, ct);
        if (!result.IsSuccess)
        {
            var error = new ValidationError(result.ErrorCode!, result.ErrorMessage!);
            return result.ErrorCode == "no_active_questionnaire"
                ? StatusCode(StatusCodes.Status503ServiceUnavailable, error)
                : BadRequest(error);
        }

        var submitted = result.Value!;
        var reflection = await _reportingService.AssembleReflectionAsync(submitted.ResponseSetId, SupportedLocales.Resolve(locale), ct);

        await _auditLogger.LogAsync("questionnaire_completed", submitted.ResponseSetId, ct: ct);
        await _auditLogger.LogAsync("personal_reflection_generated", submitted.ResponseSetId, ct: ct);

        return StatusCode(StatusCodes.Status201Created, new SubmitResponseResult(
            $"/me#{submitted.PlainToken}",
            submitted.PlainAccessCode,
            reflection));
    }
}

public sealed record AnswerRequest(Guid QuestionId, Guid PrimaryAnswerOptionId, Guid? SecondaryAnswerOptionId);
public sealed record SubmitResponseRequest(IReadOnlyList<AnswerRequest> Answers);
public sealed record SubmitResponseResult(string PrivateResultLink, string AccessCode, ReflectionDto Reflection);
internal sealed record ValidationError(string Error, string Message);
