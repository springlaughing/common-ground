using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.Api.Controllers;
using CommonGround.Api.Persistence;
using CommonGround.Modules.Audit.Entities;
using CommonGround.Modules.Comparisons.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.IntegrationTests.Infrastructure;

/// <summary>
/// Shared HTTP + DB helpers for the comparison-flow integration suites (create invite, validate,
/// join, generation), so each suite drives the flow through one place instead of re-implementing
/// the questionnaire walk and invite plumbing.
/// </summary>
internal static class ComparisonTestFlow
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // The cg_session cookie is Secure, so the test transport drops it on auto-resend; we manage it
    // manually and never auto-redirect.
    public static HttpClient NewClient(IntegrationTestFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
        });

    public static async Task<List<AnswerRequest>> ValidAnswers(HttpClient client)
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

    public static async Task<string> CompleteQuestionnaireAndGetToken(HttpClient client)
    {
        var answers = await ValidAnswers(client);
        var response = await client.PostAsJsonAsync("/api/responses", new SubmitResponseRequest(answers), JsonOptions);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("privateResultLink").GetString()!["/me#".Length..];
    }

    public static async Task<string> StartSession(HttpClient client, string token)
    {
        var response = await client.PostAsJsonAsync("/api/session/start", new { token }, JsonOptions);
        response.EnsureSuccessStatusCode();
        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("cg_session=", StringComparison.Ordinal));
        return setCookie.Split(';')[0];
    }

    /// <summary>Completes a fresh response and starts a session for it; returns the cg_session cookie.</summary>
    public static async Task<string> SessionForNewResponse(HttpClient client) =>
        await StartSession(client, await CompleteQuestionnaireAndGetToken(client));

    public static Task<HttpResponseMessage> SendWithCookie(HttpClient client, HttpMethod method, string url, string cookie)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", cookie);
        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> PostJsonWithCookie(HttpClient client, string url, object body, string cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: JsonOptions),
        };
        request.Headers.Add("Cookie", cookie);
        return client.SendAsync(request);
    }

    /// <summary>Inviter completes the questionnaire, starts a session, and mints an invite. Returns (token, comparisonId).</summary>
    public static async Task<(string Token, Guid ComparisonId)> CreateInvite(HttpClient client, string label)
    {
        var cookie = await SessionForNewResponse(client);
        var response = await PostJsonWithCookie(client, "/api/comparisons", new { inviterLabel = label }, cookie);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return (body.GetProperty("inviteToken").GetString()!, body.GetProperty("comparisonId").GetGuid());
    }

    public static Task<HttpResponseMessage> Join(HttpClient client, string token, bool consent, string inviteeLabel, IReadOnlyList<AnswerRequest> answers) =>
        client.PostAsJsonAsync("/api/invite/join", new { token, consent, inviteeLabel, answers }, JsonOptions);

    public static Task<HttpResponseMessage> Validate(HttpClient client, string token) =>
        client.PostAsJsonAsync("/api/invite/validate", new { token }, JsonOptions);

    // ── DB inspection ──────────────────────────────────────────────────────────

    public static async Task<int> AuditCount(IntegrationTestFactory factory, string eventType, Guid comparisonId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Set<AuditEvent>().CountAsync(e => e.EventType == eventType && e.ComparisonSessionId == comparisonId);
    }

    public static async Task<bool> AuditExists(IntegrationTestFactory factory, string eventType, Guid comparisonId) =>
        await AuditCount(factory, eventType, comparisonId) > 0;

    public static async Task<ComparisonStatus> SessionStatus(IntegrationTestFactory factory, Guid comparisonId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (await db.Set<ComparisonSession>().FirstAsync(s => s.Id == comparisonId)).Status;
    }

    public static async Task<ComparisonParticipant?> InviteeParticipant(IntegrationTestFactory factory, Guid comparisonId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Set<ComparisonParticipant>()
            .FirstOrDefaultAsync(p => p.ComparisonSessionId == comparisonId && p.Role == ParticipantRole.Invitee);
    }

    public static async Task ExpireInvite(IntegrationTestFactory factory, Guid comparisonId, DateTimeOffset expiresAt)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invite = await db.Set<Invite>().FirstAsync(i => i.ComparisonSessionId == comparisonId);
        invite.GetType().GetProperty(nameof(Invite.ExpiresAt))!.SetValue(invite, expiresAt);
        await db.SaveChangesAsync();
    }

    public static async Task RepinComparisonVersion(IntegrationTestFactory factory, Guid comparisonId, Guid versionId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.Set<ComparisonSession>().FirstAsync(s => s.Id == comparisonId);
        session.GetType().GetProperty(nameof(ComparisonSession.QuestionnaireVersionId))!.SetValue(session, versionId);
        await db.SaveChangesAsync();
    }
}
