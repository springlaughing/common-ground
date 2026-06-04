using CommonGround.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api/questionnaire")]
public sealed class QuestionnaireController : ControllerBase
{
    private readonly IQuestionnaireReader _reader;

    public QuestionnaireController(IQuestionnaireReader reader) => _reader = reader;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var questionnaire = await _reader.GetActiveVersionAsync(ct);
        return questionnaire is null ? NotFound() : Ok(questionnaire);
    }
}
