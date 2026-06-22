using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.Api.Controllers;
using CommonGround.Api.Persistence;
using CommonGround.IntegrationTests.Infrastructure;
using CommonGround.Modules.Audit.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.IntegrationTests;

/// <summary>
/// T012 (US1) — POST /api/comparisons mints a single-use, time-limited invite for the session's
/// response: it creates the session + initiator + invite, returns a token + pending status without
/// leaking the inviter's results, requires a session, validates the label, audits the creation, and
/// yields a distinct invite on each call.
/// </summary>
public sealed class CreateInviteTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public CreateInviteTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateInvite_WithSession_Returns201_WithTokenAndPendingStatus()
    {
        var client = NewClient();
        var cookie = await StartSessionForNewResponse(client);

        var response = await PostWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        body.GetProperty("inviteToken").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("status").GetString().Should().Be("pending");
        body.GetProperty("comparisonId").GetGuid().Should().NotBeEmpty();
        body.GetProperty("expiresAt").GetDateTimeOffset().Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task CreateInvite_ResponseDoesNotExposeInviterResults()
    {
        var client = NewClient();
        var cookie = await StartSessionForNewResponse(client);

        var response = await PostWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        // The invite link carries only the credential + status — never the inviter's reflection,
        // groups, scores, or raw answers (Principle I).
        var properties = body.EnumerateObject().Select(p => p.Name).ToList();
        properties.Should().BeEquivalentTo(["comparisonId", "inviteToken", "expiresAt", "status"]);
    }

    [Fact]
    public async Task CreateInvite_WithoutSession_Returns401()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/comparisons", new { inviterLabel = "Alex" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInvite_MissingLabel_Returns400()
    {
        var client = NewClient();
        var cookie = await StartSessionForNewResponse(client);

        var response = await PostWithCookie(client, "/api/comparisons", new { }, cookie);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_label");
    }

    [Fact]
    public async Task CreateInvite_TooLongLabel_Returns400()
    {
        var client = NewClient();
        var cookie = await StartSessionForNewResponse(client);

        var response = await PostWithCookie(client, "/api/comparisons", new { inviterLabel = new string('x', 61) }, cookie);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_label");
    }

    [Fact]
    public async Task CreateInvite_CalledTwice_YieldsDistinctInvites()
    {
        var client = NewClient();
        var cookie = await StartSessionForNewResponse(client);

        var first = await (await PostWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var second = await (await PostWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        second.GetProperty("inviteToken").GetString()
            .Should().NotBe(first.GetProperty("inviteToken").GetString());
        second.GetProperty("comparisonId").GetGuid()
            .Should().NotBe(first.GetProperty("comparisonId").GetGuid());
    }

    [Fact]
    public async Task CreateInvite_WritesComparisonInviteCreatedAudit()
    {
        var client = NewClient();
        var cookie = await StartSessionForNewResponse(client);

        var body = await (await PostWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var comparisonId = body.GetProperty("comparisonId").GetGuid();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var audit = await db.Set<AuditEvent>()
            .FirstOrDefaultAsync(e => e.EventType == "comparison_invite_created" && e.ComparisonSessionId == comparisonId);

        audit.Should().NotBeNull();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    // The cg_session cookie is Secure, so the test transport would drop it on auto-resend;
    // we manage it manually (same approach as ReflectionAccessTests).
    private HttpClient NewClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static async Task<string> StartSessionForNewResponse(HttpClient client)
    {
        var token = await CompleteQuestionnaireAndGetToken(client);
        var response = await client.PostAsJsonAsync("/api/session/start", new { token }, JsonOptions);
        response.EnsureSuccessStatusCode();
        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("cg_session=", StringComparison.Ordinal));
        return setCookie.Split(';')[0];
    }

    private static Task<HttpResponseMessage> PostWithCookie(HttpClient client, string url, object body, string cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: JsonOptions),
        };
        request.Headers.Add("Cookie", cookie);
        return client.SendAsync(request);
    }

    private static async Task<string> CompleteQuestionnaireAndGetToken(HttpClient client)
    {
        var questionnaire = await (await client.GetAsync("/api/questionnaire/current"))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        var answers = questionnaire.GetProperty("questions").EnumerateArray()
            .Select(q => new AnswerRequest(
                q.GetProperty("id").GetGuid(),
                q.GetProperty("answerOptions")[0].GetProperty("id").GetGuid(),
                null))
            .ToList();

        var response = await client.PostAsJsonAsync("/api/responses", new SubmitResponseRequest(answers), JsonOptions);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var link = body.GetProperty("privateResultLink").GetString()!;
        return link["/me#".Length..];
    }
}
