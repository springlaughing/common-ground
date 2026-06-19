using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.Api.Controllers;
using CommonGround.Api.Persistence;
using CommonGround.IntegrationTests.Infrastructure;
using CommonGround.Modules.Responses.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.IntegrationTests;

public sealed class ReflectionAccessTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ReflectionAccessTests(IntegrationTestFactory factory) : base(factory) { }

    // ─── POST /api/session/start ──────────────────────────────────────────────

    [Fact]
    public async Task SessionStart_ValidToken_Returns200_AndSetsSessionCookie()
    {
        var client = NewClient();
        var token = await CompleteQuestionnaireAndGetToken(client);

        var response = await client.PostAsJsonAsync("/api/session/start", new { token }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("cg_session=", StringComparison.Ordinal));
        setCookie.Should().ContainEquivalentOf("httponly");
        setCookie.Should().ContainEquivalentOf("samesite=strict");
    }

    [Fact]
    public async Task SessionStart_InvalidToken_Returns401()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/session/start", new { token = "not-a-real-token" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_token");
    }

    // ─── GET /api/me/reflection ─────────────────────────────────────────────────

    [Fact]
    public async Task MeReflection_WithSession_Returns200_WithReflectionAndAccessCodeFlag()
    {
        var client = NewClient();
        var token = await CompleteQuestionnaireAndGetToken(client);
        var sessionCookie = await StartSession(client, token);

        var response = await SendWithCookie(client, HttpMethod.Get, "/api/me/reflection", sessionCookie);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        body.GetProperty("accessCodeAvailable").GetBoolean().Should().BeTrue();
        // Group membership is score/threshold dependent (covered by the assembler unit
        // tests); here we assert the access-controlled shape is correct.
        body.GetProperty("reflection").GetProperty("groups").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task MeReflection_WithoutSession_Returns401()
    {
        var client = NewClient();

        var response = await client.GetAsync("/api/me/reflection");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MeReflection_SoftDeletedResponse_Returns404()
    {
        var client = NewClient();
        var token = await CompleteQuestionnaireAndGetToken(client);
        var sessionCookie = await StartSession(client, token);

        await SoftDeleteLatestResponseSet();

        var response = await SendWithCookie(client, HttpMethod.Get, "/api/me/reflection", sessionCookie);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── T037 (US4) — GET /api/me/reflection?locale= re-renders at view time ────

    [Fact]
    public async Task MeReflection_LocaleDe_ReturnsGermanReflection_SameInsightsOrderAndStrengthAsEnglish()
    {
        var client = NewClient();
        var token = await CompleteQuestionnaireAndGetToken(client);
        var sessionCookie = await StartSession(client, token);

        // No locale is stored on the response; the saved reflection is re-assembled per
        // request, so the same session yields the same insights re-rendered in whichever
        // locale is asked for (en↔de).
        var en = await GetMeReflection(client, sessionCookie, "en");
        var de = await GetMeReflection(client, sessionCookie, "de");

        // SC-003 carried to the saved reflection: identical answers ⇒ identical insight
        // selection, order, and strength regardless of the view-time locale.
        var enShape = Shape(en);
        enShape.Should().NotBeEmpty();
        Shape(de).Should().BeEquivalentTo(enShape, o => o.WithStrictOrdering());

        // Localized: for the same ids, German group titles and insight text differ from English.
        var enGroups = en.GetProperty("groups").EnumerateArray().ToList();
        var deGroups = de.GetProperty("groups").EnumerateArray().ToList();
        for (var g = 0; g < enGroups.Count; g++)
        {
            deGroups[g].GetProperty("id").GetString().Should().Be(enGroups[g].GetProperty("id").GetString());
            deGroups[g].GetProperty("title").GetString()
                .Should().NotBe(enGroups[g].GetProperty("title").GetString());

            var enInsights = enGroups[g].GetProperty("insights").EnumerateArray().ToList();
            var deInsights = deGroups[g].GetProperty("insights").EnumerateArray().ToList();
            for (var i = 0; i < enInsights.Count; i++)
            {
                deInsights[i].GetProperty("dimensionId").GetString()
                    .Should().Be(enInsights[i].GetProperty("dimensionId").GetString());
                deInsights[i].GetProperty("text").GetString()
                    .Should().NotBe(enInsights[i].GetProperty("text").GetString());
            }
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    // A no-cookie client: the cg_session cookie is Secure, so the test-server's
    // HTTP transport would drop it on auto-resend — we manage it manually instead.
    private HttpClient NewClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static async Task<string> StartSession(HttpClient client, string token)
    {
        var response = await client.PostAsJsonAsync("/api/session/start", new { token }, JsonOptions);
        response.EnsureSuccessStatusCode();
        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("cg_session=", StringComparison.Ordinal));
        return setCookie.Split(';')[0]; // "cg_session=<jwt>"
    }

    private static Task<HttpResponseMessage> SendWithCookie(HttpClient client, HttpMethod method, string url, string cookie)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", cookie);
        return client.SendAsync(request);
    }

    private async Task SoftDeleteLatestResponseSet()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var responseSet = await db.Set<ResponseSet>().OrderByDescending(r => r.CreatedAt).FirstAsync();
        responseSet.IsDeleted = true;
        responseSet.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
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
        var link = body.GetProperty("privateResultLink").GetString()!; // "/me#<token>"
        return link["/me#".Length..];
    }

    private static async Task<JsonElement> GetMeReflection(HttpClient client, string cookie, string locale)
    {
        var response = await SendWithCookie(client, HttpMethod.Get, $"/api/me/reflection?locale={locale}", cookie);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("reflection");
    }

    // A locale-invariant fingerprint of a reflection: ordered (groupId, [(dimensionId, strength)]).
    private static List<(string Group, List<(string Dimension, int Strength)> Insights)> Shape(JsonElement reflection) =>
        reflection.GetProperty("groups").EnumerateArray()
            .Select(g => (
                Group: g.GetProperty("id").GetString()!,
                Insights: g.GetProperty("insights").EnumerateArray()
                    .Select(i => (
                        Dimension: i.GetProperty("dimensionId").GetString()!,
                        Strength: i.GetProperty("strength").GetInt32()))
                    .ToList()))
            .ToList();
}
