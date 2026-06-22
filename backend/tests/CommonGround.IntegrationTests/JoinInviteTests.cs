using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.Api.Controllers;
using CommonGround.Api.Persistence;
using CommonGround.IntegrationTests.Infrastructure;
using CommonGround.Modules.Audit.Entities;
using CommonGround.Modules.Comparisons.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        var client = NewClient();
        var (token, _) = await CreateInvite(client, "Alex");

        var response = await client.PostAsJsonAsync("/api/invite/validate", new { token }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("inviterLabel").GetString().Should().Be("Alex");
        body.GetProperty("status").GetString().Should().Be("active");

        // Not consumed: a second validate still succeeds.
        var again = await client.PostAsJsonAsync("/api/invite/validate", new { token }, JsonOptions);
        again.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Validate_UnknownToken_Returns404()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/invite/validate", new { token = "nope" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Validate_WritesComparisonInviteOpenedAudit()
    {
        var client = NewClient();
        var (token, comparisonId) = await CreateInvite(client, "Alex");

        await client.PostAsJsonAsync("/api/invite/validate", new { token }, JsonOptions);

        (await AuditExists("comparison_invite_opened", comparisonId)).Should().BeTrue();
    }

    // ─── Join ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Join_WithoutConsent_CreatesNothing_Returns400()
    {
        var client = NewClient();
        var (token, _) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        var response = await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = false, inviteeLabel = "Sam", answers }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("consent_required");

        // Nothing consumed — the invite is still joinable afterwards.
        var validate = await client.PostAsJsonAsync("/api/invite/validate", new { token }, JsonOptions);
        validate.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Join_WithConsent_ReturnsOwnCredentials_AndConsumesInvite()
    {
        var client = NewClient();
        var (token, comparisonId) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        var response = await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("privateResultLink").GetString().Should().StartWith("/me#");
        body.GetProperty("accessCode").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("comparisonId").GetGuid().Should().Be(comparisonId);

        // Starts the invitee's session.
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.StartsWith("cg_session=", StringComparison.Ordinal));

        // Single-use: a second join with the same token is rejected.
        var second = await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_RecordsInviteeParticipantWithLabel_AndWritesJoinedAudit()
    {
        var client = NewClient();
        var (token, comparisonId) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invitee = await db.Set<ComparisonParticipant>()
            .FirstOrDefaultAsync(p => p.ComparisonSessionId == comparisonId && p.Role == ParticipantRole.Invitee);

        invitee.Should().NotBeNull();
        invitee!.DisplayLabel.Should().Be("Sam");
        (await AuditExists("comparison_joined", comparisonId)).Should().BeTrue();
    }

    [Fact]
    public async Task Join_GivesInviteeAWorkingPrivateResultLink()
    {
        var client = NewClient();
        var (token, _) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        var body = await (await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var inviteeToken = body.GetProperty("privateResultLink").GetString()!["/me#".Length..];

        // The invitee's own token starts a session and loads their own reflection — proving they got
        // their own scored response (not the inviter's).
        var sessionCookie = await StartSession(client, inviteeToken);
        var reflection = await SendWithCookie(client, HttpMethod.Get, "/api/me/reflection", sessionCookie);
        reflection.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Join_UsedInvite_Returns409()
    {
        var client = NewClient();
        var (token, _) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions);

        var second = await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Jordan", answers }, JsonOptions);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_ExpiredBeyondGrace_Returns409()
    {
        var client = NewClient();
        var (token, comparisonId) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        await ExpireInvite(comparisonId, DateTimeOffset.UtcNow - TimeSpan.FromHours(2)); // past the 1h grace

        var response = await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Join_VersionMismatch_Returns400()
    {
        var client = NewClient();
        var (token, comparisonId) = await CreateInvite(client, "Alex");
        var answers = await ValidAnswers(client);

        await RepinComparisonVersion(comparisonId, Guid.NewGuid()); // simulate the active version moving on

        var response = await client.PostAsJsonAsync("/api/invite/join",
            new { token, consent = true, inviteeLabel = "Sam", answers }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("version_mismatch");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private HttpClient NewClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    // Inviter completes the questionnaire, starts a session, and mints an invite. Returns (token, comparisonId).
    private static async Task<(string Token, Guid ComparisonId)> CreateInvite(HttpClient client, string label)
    {
        var privateToken = await CompleteQuestionnaireAndGetToken(client);
        var cookie = await StartSession(client, privateToken);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/comparisons")
        {
            Content = JsonContent.Create(new { inviterLabel = label }, options: JsonOptions),
        };
        request.Headers.Add("Cookie", cookie);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return (body.GetProperty("inviteToken").GetString()!, body.GetProperty("comparisonId").GetGuid());
    }

    private static async Task<string> StartSession(HttpClient client, string token)
    {
        var response = await client.PostAsJsonAsync("/api/session/start", new { token }, JsonOptions);
        response.EnsureSuccessStatusCode();
        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("cg_session=", StringComparison.Ordinal));
        return setCookie.Split(';')[0];
    }

    private static Task<HttpResponseMessage> SendWithCookie(HttpClient client, HttpMethod method, string url, string cookie)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", cookie);
        return client.SendAsync(request);
    }

    private static async Task<List<AnswerRequest>> ValidAnswers(HttpClient client)
    {
        var questionnaire = await (await client.GetAsync("/api/questionnaire/current"))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        return questionnaire.GetProperty("questions").EnumerateArray()
            .Select(q => new AnswerRequest(
                q.GetProperty("id").GetGuid(),
                q.GetProperty("answerOptions")[0].GetProperty("id").GetGuid(),
                null))
            .ToList();
    }

    private static async Task<string> CompleteQuestionnaireAndGetToken(HttpClient client)
    {
        var answers = await ValidAnswers(client);
        var response = await client.PostAsJsonAsync("/api/responses", new SubmitResponseRequest(answers), JsonOptions);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("privateResultLink").GetString()!["/me#".Length..];
    }

    private async Task<bool> AuditExists(string eventType, Guid comparisonId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Set<AuditEvent>()
            .AnyAsync(e => e.EventType == eventType && e.ComparisonSessionId == comparisonId);
    }

    private async Task ExpireInvite(Guid comparisonId, DateTimeOffset expiresAt)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invite = await db.Set<Invite>().FirstAsync(i => i.ComparisonSessionId == comparisonId);
        invite.GetType().GetProperty(nameof(Invite.ExpiresAt))!.SetValue(invite, expiresAt);
        await db.SaveChangesAsync();
    }

    private async Task RepinComparisonVersion(Guid comparisonId, Guid versionId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.Set<ComparisonSession>().FirstAsync(s => s.Id == comparisonId);
        session.GetType().GetProperty(nameof(ComparisonSession.QuestionnaireVersionId))!.SetValue(session, versionId);
        await db.SaveChangesAsync();
    }
}
