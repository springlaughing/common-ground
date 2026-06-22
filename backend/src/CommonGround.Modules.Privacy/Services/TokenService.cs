using System.Security.Cryptography;
using System.Text;
using CommonGround.SharedKernel.Interfaces;

namespace CommonGround.Modules.Privacy.Services;

public sealed class TokenService : ITokenService
{
    private readonly byte[] _hmacKey;

    public TokenService(byte[] hmacKey)
    {
        _hmacKey = hmacKey;
    }

    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    // ITokenService bridge — lets feature modules depend on the SharedKernel abstraction
    // (they may not reference Privacy). Delegates to the static primitive above; HashToken
    // is already an instance method and satisfies the interface directly.
    string ITokenService.GenerateToken() => GenerateToken();

    public static string GenerateAccessCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(8);
        var hex = Convert.ToHexString(bytes).ToUpperInvariant();
        return $"{hex[..4]}-{hex[4..8]}-{hex[8..12]}";
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hash = HMACSHA256.HashData(_hmacKey, tokenBytes);
        return Convert.ToBase64String(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
