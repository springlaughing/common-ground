using CommonGround.SharedKernel.Interfaces;
using CommonGround.SharedKernel.Localization;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api/questionnaire")]
public sealed class QuestionnaireController : ControllerBase
{
    private readonly IQuestionnaireReader _reader;

    public QuestionnaireController(IQuestionnaireReader reader) => _reader = reader;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent([FromQuery] string? locale, CancellationToken ct)
    {
        var questionnaire = await _reader.GetActiveVersionAsync(SupportedLocales.Resolve(locale), ct);
        return questionnaire is null ? NotFound() : Ok(questionnaire);
    }
}
