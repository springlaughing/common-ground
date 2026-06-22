namespace CommonGround.SharedKernel.Interfaces;

/// <summary>
/// The shared credential <b>primitive</b>: high-entropy token generation and HMAC-SHA256 hashing.
/// Exposed through SharedKernel so feature modules (which may not reference one another) can reach
/// it without depending on the Privacy module directly. Token <i>policy</i> (expiry windows,
/// single-use lifecycle) lives in the owning module's service, never here.
/// </summary>
public interface ITokenService
{
    /// <summary>A fresh, URL-safe, high-entropy token. The plain value is shared only in a link
    /// fragment; only its hash is ever stored.</summary>
    string GenerateToken();

    /// <summary>HMAC-SHA256 of a token, for storage and constant-key lookup. Never reversible to the plain token.</summary>
    string HashToken(string token);
}
