using CommonGround.Modules.Privacy.Services;
using CommonGround.Modules.Reporting;
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
    private readonly IQuestionnaireReader _questionnaireReader;
    private readonly IResponseRepository _responseRepository;
    private readonly ScoringEngine _scoringEngine;
    private readonly IReportingService _reportingService;
    private readonly TokenService _tokenService;
    private readonly IAuditLogger _auditLogger;

    public ResponsesController(
        IQuestionnaireReader questionnaireReader,
        IResponseRepository responseRepository,
        ScoringEngine scoringEngine,
        IReportingService reportingService,
        TokenService tokenService,
        IAuditLogger auditLogger)
    {
        _questionnaireReader = questionnaireReader;
        _responseRepository = responseRepository;
        _scoringEngine = scoringEngine;
        _reportingService = reportingService;
        _tokenService = tokenService;
        _auditLogger = auditLogger;
    }

    [HttpPost]
    [EnableRateLimiting("PostPolicy")]
    [Consumes("application/json")]
    public async Task<IActionResult> Submit(SubmitResponseRequest request, [FromQuery] string? locale, CancellationToken ct)
    {
        // Validation only compares stable IDs, so the default locale is sufficient here;
        // the requested locale localizes the returned reflection below.
        var questionnaire = await _questionnaireReader.GetActiveVersionAsync(SupportedLocales.Default, ct);
        if (questionnaire is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ValidationError("no_active_questionnaire", "No active questionnaire is available."));

        var error = Validate(request.Answers, questionnaire);
        if (error is not null)
            return BadRequest(error);

        var token = TokenService.GenerateToken();
        var accessCode = TokenService.GenerateAccessCode();

        var answerInputs = request.Answers
            .Select(a => new AnswerInput(a.QuestionId, a.PrimaryAnswerOptionId, a.SecondaryAnswerOptionId))
            .ToList();

        var responseSet = await _responseRepository.CreateAsync(
            questionnaire.Id,
            _tokenService.HashToken(token),
            _tokenService.HashToken(accessCode),
            answerInputs,
            ct);

        var scoringInputs = request.Answers
            .Select(a => new ScoringInput(a.PrimaryAnswerOptionId, a.SecondaryAnswerOptionId))
            .ToList();

        await _scoringEngine.ScoreAsync(responseSet.Id, questionnaire.Id, scoringInputs, ct);

        var reflection = await _reportingService.AssembleReflectionAsync(responseSet.Id, SupportedLocales.Resolve(locale), ct);

        await _auditLogger.LogAsync("questionnaire_completed", responseSet.Id, ct: ct);
        await _auditLogger.LogAsync("personal_reflection_generated", responseSet.Id, ct: ct);

        return StatusCode(StatusCodes.Status201Created, new SubmitResponseResult(
            $"/me#{token}",
            accessCode,
            reflection));
    }

    private static ValidationError? Validate(
        IReadOnlyList<AnswerRequest> answers,
        ActiveQuestionnaireDto questionnaire)
    {
        var seenQuestions = answers.Select(a => a.QuestionId).ToHashSet();
        if (seenQuestions.Count != answers.Count)
            return new ValidationError("duplicate_question_ids", "Each question must be answered exactly once.");

        var requiredIds = questionnaire.Questions.Select(q => q.Id).ToHashSet();
        if (!requiredIds.SetEquals(seenQuestions))
            return new ValidationError("incomplete_answers", "All questions must be answered before submitting.");

        var optionsByQuestion = questionnaire.Questions
            .ToDictionary(q => q.Id, q => q.AnswerOptions.Select(o => o.Id).ToHashSet());

        foreach (var answer in answers)
        {
            var error = ValidateAnswerOptions(answer, optionsByQuestion);
            if (error is not null)
                return error;
        }

        return null;
    }

    private static ValidationError? ValidateAnswerOptions(
        AnswerRequest answer,
        Dictionary<Guid, HashSet<Guid>> optionsByQuestion)
    {
        if (!optionsByQuestion.TryGetValue(answer.QuestionId, out var validOptions))
            return new ValidationError("invalid_question_id", "Answer references a question not in the active questionnaire.");

        if (!validOptions.Contains(answer.PrimaryAnswerOptionId))
            return new ValidationError("invalid_answer_option", "Primary answer option does not belong to the specified question.");

        if (!answer.SecondaryAnswerOptionId.HasValue)
            return null;

        if (!validOptions.Contains(answer.SecondaryAnswerOptionId.Value))
            return new ValidationError("invalid_answer_option", "Secondary answer option does not belong to the specified question.");

        if (answer.SecondaryAnswerOptionId.Value == answer.PrimaryAnswerOptionId)
            return new ValidationError("duplicate_answer_option", "Secondary answer option must differ from the primary.");

        return null;
    }
}

public sealed record AnswerRequest(Guid QuestionId, Guid PrimaryAnswerOptionId, Guid? SecondaryAnswerOptionId);
public sealed record SubmitResponseRequest(IReadOnlyList<AnswerRequest> Answers);
public sealed record SubmitResponseResult(string PrivateResultLink, string AccessCode, ReflectionDto Reflection);
internal sealed record ValidationError(string Error, string Message);
