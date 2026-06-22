using CommonGround.SharedKernel.Interfaces;

namespace CommonGround.Modules.Comparisons.Services;

/// <summary>
/// Owns invite-token <b>policy</b> — the fixed validity window — over the shared token
/// <see cref="ITokenService"/> primitive. Generation and hashing themselves live in the primitive;
/// this service decides how long an invite lives. The single-use Active→Used lifecycle is enforced
/// at join time (US2).
/// </summary>
public sealed class InviteTokenService
{
    /// <summary>How long an invite is valid from creation. A short grace period past this still
    /// admits an in-progress join (applied at join, US2).</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private readonly ITokenService _tokenService;

    public InviteTokenService(ITokenService tokenService) => _tokenService = tokenService;

    /// <summary>Mints a fresh single-use invite credential: the plain token (shared in the link
    /// fragment), its stored hash, and the creation + expiry stamps.</summary>
    public IssuedInviteToken Issue()
    {
        var now = DateTimeOffset.UtcNow;
        var plainToken = _tokenService.GenerateToken();
        return new IssuedInviteToken(
            plainToken,
            _tokenService.HashToken(plainToken),
            now,
            now + Lifetime);
    }

    /// <summary>Hashes a token presented later (validate/join, US2) for lookup against the stored hash.</summary>
    public string Hash(string token) => _tokenService.HashToken(token);
}

/// <param name="PlainToken">Shared only in the <c>/invite#TOKEN</c> fragment; never persisted.</param>
/// <param name="TokenHash">The only form stored server-side.</param>
public sealed record IssuedInviteToken(string PlainToken, string TokenHash, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt);
