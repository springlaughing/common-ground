using CommonGround.Api.Auth;
using CommonGround.Modules.Privacy.Services;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CommonGround.Api.Controllers;

[ApiController]
[Route("api/session")]
public sealed class SessionController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IResponseReader _responseReader;
    private readonly SessionTokenIssuer _tokenIssuer;

    public SessionController(
        TokenService tokenService,
        IResponseReader responseReader,
        SessionTokenIssuer tokenIssuer)
    {
        _tokenService = tokenService;
        _responseReader = responseReader;
        _tokenIssuer = tokenIssuer;
    }

    // T032 — validates a private result token and starts a session by issuing
    // the HttpOnly cg_session cookie. The plain token never leaves the client
    // except in this body; only its HMAC hash is stored server-side.
    [HttpPost("start")]
    [EnableRateLimiting("PostPolicy")]
    [Consumes("application/json")]
    public async Task<IActionResult> Start(StartSessionRequest request, CancellationToken ct)
    {
        var invalid = new ValidationError("invalid_token", "This result link is not valid or has been deleted.");

        if (string.IsNullOrWhiteSpace(request.Token))
            return Unauthorized(invalid);

        var tokenHash = _tokenService.HashToken(request.Token);
        var responseSet = await _responseReader.GetByTokenHashAsync(tokenHash, ct);
        if (responseSet is null || responseSet.IsDeleted)
            return Unauthorized(invalid);

        var (jwt, maxAge) = _tokenIssuer.Issue(responseSet.Id);
        Response.Cookies.Append("cg_session", jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = maxAge,
            Path = "/",
        });

        return Ok();
    }
}

public sealed record StartSessionRequest(string Token);
