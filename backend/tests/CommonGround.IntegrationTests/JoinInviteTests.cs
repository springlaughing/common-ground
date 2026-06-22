using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CommonGround.IntegrationTests;

/// <summary>
/// T020 (US2) — validating an invite never consumes it; declining creates nothing; consenting +
/// completing creates the invitee's own response + Invitee participant, consumes the invite
/// single-use, returns the invitee's own credentials, and starts their session. Used/expired
/// (outside grace) and version mismatch are rejected; lifecycle audits are written.
/// </summary>
public sealed class JoinInviteTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public JoinInviteTests(IntegrationTestFactory factory) : base(factory) { }

    // ─── Validate ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ReturnsInviterLabel_WithoutConsumingTheInvite()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");

        var response = await ComparisonTestFlow.Validate(client, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("inviterLabel").GetString().Should().Be("Alex");
        body.GetProperty("status").GetString().Should().Be("active");

        // Not consumed: a second validate still succeeds.
        (await ComparisonTestFlow.Validate(client, token)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Validate_UnknownToken_Returns404()
    {
        var client = ComparisonTestFlow.NewClient(Factory);

        var response = await ComparisonTestFlow.Validate(client, "nope");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Validate_WritesComparisonInviteOpenedAudit()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId) = await ComparisonTestFlow.CreateInvite(client, "Alex");

        await ComparisonTestFlow.Validate(client, token);

        (await ComparisonTestFlow.AuditExists(Factory, "comparison_invite_opened", comparisonId)).Should().BeTrue();
    }

    // ─── Join ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Join_WithoutConsent_CreatesNothing_Returns400()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        var response = await ComparisonTestFlow.Join(client, token, consent: false, "Sam", answers);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("consent_required");

        // Nothing consumed — the invite is still joinable afterwards.
        (await ComparisonTestFlow.Validate(client, token)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Join_WithConsent_ReturnsOwnCredentials_AndConsumesInvite()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        var response = await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("privateResultLink").GetString().Should().StartWith("/me#");
        body.GetProperty("accessCode").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("comparisonId").GetGuid().Should().Be(comparisonId);

        // Starts the invitee's session.
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.StartsWith("cg_session=", StringComparison.Ordinal));

        // Single-use: a second join with the same token is rejected.
        var second = await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_RecordsInviteeParticipantWithLabel_AndWritesJoinedAudit()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);

        var invitee = await ComparisonTestFlow.InviteeParticipant(Factory, comparisonId);
        invitee.Should().NotBeNull();
        invitee!.DisplayLabel.Should().Be("Sam");
        (await ComparisonTestFlow.AuditExists(Factory, "comparison_joined", comparisonId)).Should().BeTrue();
    }

    [Fact]
    public async Task Join_GivesInviteeAWorkingPrivateResultLink()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        var body = await (await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var inviteeToken = body.GetProperty("privateResultLink").GetString()!["/me#".Length..];

        // The invitee's own token starts a session and loads their own reflection — proving they got
        // their own scored response (not the inviter's).
        var sessionCookie = await ComparisonTestFlow.StartSession(client, inviteeToken);
        var reflection = await ComparisonTestFlow.SendWithCookie(client, HttpMethod.Get, "/api/me/reflection", sessionCookie);
        reflection.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Join_UsedInvite_Returns409()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);
        var second = await ComparisonTestFlow.Join(client, token, consent: true, "Jordan", answers);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_ExpiredBeyondGrace_Returns409()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        await ComparisonTestFlow.ExpireInvite(Factory, comparisonId, DateTimeOffset.UtcNow - TimeSpan.FromHours(2)); // past the 1h grace

        var response = await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_VersionMismatch_Returns400()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        await ComparisonTestFlow.RepinComparisonVersion(Factory, comparisonId, Guid.NewGuid()); // simulate the active version moving on

        var response = await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("version_mismatch");
    }
}
