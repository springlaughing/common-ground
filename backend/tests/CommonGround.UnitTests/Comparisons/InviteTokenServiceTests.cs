using System.Text;
using CommonGround.Modules.Comparisons.Services;
using CommonGround.Modules.Privacy.Services;
using FluentAssertions;

namespace CommonGround.UnitTests.Comparisons;

/// <summary>
/// T011 (US1) — invite-token primitive + policy: tokens are unique and high-entropy, only the hash
/// is ever exposed for storage, and the expiry window is the fixed lifetime from creation.
/// </summary>
public sealed class InviteTokenServiceTests
{
    private static readonly TokenService TokenService =
        new(Encoding.UTF8.GetBytes("unit-test-hmac-key-minimum-32-characters"));

    private static InviteTokenService NewService() => new(TokenService);

    [Fact]
    public void Issue_ProducesPlainTokenAndItsHash()
    {
        var issued = NewService().Issue();

        issued.PlainToken.Should().NotBeNullOrWhiteSpace();
        issued.TokenHash.Should().NotBeNullOrWhiteSpace();

        // The stored form is the hash of the plain token — never the plain token itself.
        issued.TokenHash.Should().NotBe(issued.PlainToken);
        issued.TokenHash.Should().Be(TokenService.HashToken(issued.PlainToken));
    }

    [Fact]
    public void Issue_ProducesADistinctTokenEachCall()
    {
        var first = NewService().Issue();
        var second = NewService().Issue();

        second.PlainToken.Should().NotBe(first.PlainToken);
        second.TokenHash.Should().NotBe(first.TokenHash);
    }

    [Fact]
    public void Issue_SetsExpiryToTheFixedLifetimeAfterCreation()
    {
        var before = DateTimeOffset.UtcNow;
        var issued = NewService().Issue();
        var after = DateTimeOffset.UtcNow;

        (issued.ExpiresAt - issued.CreatedAt).Should().Be(InviteTokenService.Lifetime);
        issued.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        issued.ExpiresAt.Should().BeAfter(issued.CreatedAt);
    }

    [Fact]
    public void Hash_IsDeterministicAndMatchesThePrimitive()
    {
        var service = NewService();
        const string token = "a-presented-token";

        var first = service.Hash(token);

        first.Should().Be(service.Hash(token));
        first.Should().Be(TokenService.HashToken(token));
    }
}
