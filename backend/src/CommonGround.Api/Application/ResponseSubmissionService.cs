using CommonGround.Modules.Privacy.Services;
using CommonGround.Modules.Reporting;
using CommonGround.Modules.Responses.Services;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;

namespace CommonGround.Api.Application;

/// <summary>
/// The shared submission pipeline behind both the normal questionnaire submit
/// (<c>POST /api/responses</c>) and the invitee join (<c>POST /api/invite/join</c>): validate the
/// answers against the active questionnaire, mint the private token + access code, persist the
/// response, and score it. Centralizing it guarantees every response — however it arrives — is
/// validated and scored <i>identically</i>, which the deterministic comparison engine depends on.
/// Cross-module orchestration (Responses + Reporting + Privacy) belongs to the Api host, not to any
/// single feature module.
/// </summary>
public sealed class ResponseSubmissionService
{
    private readonly IQuestionnaireReader _questionnaireReader;
    private readonly IResponseRepository _responseRepository;
    private readonly ScoringEngine _scoringEngine;
    private readonly TokenService _tokenService;

    public ResponseSubmissionService(
        IQuestionnaireReader questionnaireReader,
        IResponseRepository responseRepository,
        ScoringEngine scoringEngine,
        TokenService tokenService)
    {
        _questionnaireReader = questionnaireReader;
        _responseRepository = responseRepository;
        _scoringEngine = scoringEngine;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Validates, persists, and scores a response. The answers are checked against the active
    /// questionnaire (validation compares stable IDs, so the default locale suffices). Does NOT
    /// commit a transaction — callers own the unit of work, so the invitee join can wrap this plus
    /// invite consumption atomically.
    /// </summary>
    public async Task<Result<SubmittedResponse>> SubmitAsync(
        IReadOnlyList<AnswerInput> answers,
        CancellationToken ct = default)
    {
        var questionnaire = await _questionnaireReader.GetActiveVersionAsync(SupportedLocales.Default, ct);
        if (questionnaire is null)
            return Result.Failure<SubmittedResponse>("no_active_questionnaire", "No active questionnaire is available.");

        var error = Validate(answers, questionnaire);
        if (error is not null)
            return Result.Failure<SubmittedResponse>(error.Value.Code, error.Value.Message);

        var token = TokenService.GenerateToken();
        var accessCode = TokenService.GenerateAccessCode();

        var responseSet = await _responseRepository.CreateAsync(
            questionnaire.Id,
            _tokenService.HashToken(token),
            _tokenService.HashToken(accessCode),
            answers,
            ct);

        var scoringInputs = answers
            .Select(a => new ScoringInput(a.PrimaryAnswerOptionId, a.SecondaryAnswerOptionId))
            .ToList();
        await _scoringEngine.ScoreAsync(responseSet.Id, questionnaire.Id, scoringInputs, ct);

        return Result.Success(new SubmittedResponse(responseSet.Id, questionnaire.Id, token, accessCode));
    }

    private static (string Code, string Message)? Validate(
        IReadOnlyList<AnswerInput> answers,
        ActiveQuestionnaireDto questionnaire)
    {
        var seenQuestions = answers.Select(a => a.QuestionId).ToHashSet();
        if (seenQuestions.Count != answers.Count)
            return ("duplicate_question_ids", "Each question must be answered exactly once.");

        var requiredIds = questionnaire.Questions.Select(q => q.Id).ToHashSet();
        if (!requiredIds.SetEquals(seenQuestions))
            return ("incomplete_answers", "All questions must be answered before submitting.");

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

    private static (string Code, string Message)? ValidateAnswerOptions(
        AnswerInput answer,
        Dictionary<Guid, HashSet<Guid>> optionsByQuestion)
    {
        if (!optionsByQuestion.TryGetValue(answer.QuestionId, out var validOptions))
            return ("invalid_question_id", "Answer references a question not in the active questionnaire.");

        if (!validOptions.Contains(answer.PrimaryAnswerOptionId))
            return ("invalid_answer_option", "Primary answer option does not belong to the specified question.");

        if (!answer.SecondaryAnswerOptionId.HasValue)
            return null;

        if (!validOptions.Contains(answer.SecondaryAnswerOptionId.Value))
            return ("invalid_answer_option", "Secondary answer option does not belong to the specified question.");

        if (answer.SecondaryAnswerOptionId.Value == answer.PrimaryAnswerOptionId)
            return ("duplicate_answer_option", "Secondary answer option must differ from the primary.");

        return null;
    }
}

/// <param name="ResponseSetId">The newly created response.</param>
/// <param name="QuestionnaireVersionId">The active version it was created on (for same-version checks).</param>
/// <param name="PlainToken">The private result token — shown once as <c>/me#&lt;token&gt;</c>; only its hash is stored.</param>
/// <param name="PlainAccessCode">The access code — shown once; only its hash is stored.</param>
public sealed record SubmittedResponse(Guid ResponseSetId, Guid QuestionnaireVersionId, string PlainToken, string PlainAccessCode);
