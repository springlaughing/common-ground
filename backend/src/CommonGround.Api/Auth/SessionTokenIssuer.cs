using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CommonGround.Api.Auth;

/// <summary>
/// Mints the short-lived <c>cg_session</c> JWT used by the returning-user flow.
/// The signing key + issuer/audience/lifetime come from the same Jwt:* config
/// that AddJwtBearer validates against in Program.cs, so issued tokens verify.
/// </summary>
public sealed class SessionTokenIssuer
{
    private readonly IConfiguration _config;

    public SessionTokenIssuer(IConfiguration config) => _config = config;

    public (string Token, TimeSpan MaxAge) Issue(Guid responseSetId)
    {
        var secret = _config["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is required");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var lifetime = TimeSpan.FromDays(int.TryParse(_config["Jwt:ExpiryDays"], out var days) ? days : 30);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: [new Claim("sub", responseSetId.ToString())],
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), lifetime);
    }
}
