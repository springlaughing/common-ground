using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.IntegrationTests.Infrastructure;
using FluentAssertions;

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
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var response = await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie);

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
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var response = await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        // The invite link carries only the credential + status — never the inviter's reflection,
        // groups, scores, or raw answers (Principle I).
        var properties = body.EnumerateObject().Select(p => p.Name).ToList();
        properties.Should().BeEquivalentTo(["comparisonId", "inviteToken", "expiresAt", "status"]);
    }

    [Fact]
    public async Task CreateInvite_WithoutSession_Returns401()
    {
        var client = ComparisonTestFlow.NewClient(Factory);

        var response = await client.PostAsJsonAsync("/api/comparisons", new { inviterLabel = "Alex" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInvite_MissingLabel_Returns400()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var response = await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { }, cookie);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_label");
    }

    [Fact]
    public async Task CreateInvite_TooLongLabel_Returns400()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var response = await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = new string('x', 61) }, cookie);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_label");
    }

    [Fact]
    public async Task CreateInvite_CalledTwice_YieldsDistinctInvites()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var first = await (await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var second = await (await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        second.GetProperty("inviteToken").GetString()
            .Should().NotBe(first.GetProperty("inviteToken").GetString());
        second.GetProperty("comparisonId").GetGuid()
            .Should().NotBe(first.GetProperty("comparisonId").GetGuid());
    }

    [Fact]
    public async Task CreateInvite_WritesComparisonInviteCreatedAudit()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var body = await (await ComparisonTestFlow.PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = "Alex" }, cookie))
            .Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var comparisonId = body.GetProperty("comparisonId").GetGuid();

        (await ComparisonTestFlow.AuditExists(Factory, "comparison_invite_created", comparisonId)).Should().BeTrue();
    }
}
