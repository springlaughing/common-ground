using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CommonGround.IntegrationTests;

/// <summary>
/// T034 (US4) — GET /api/me/comparisons lists the caller's comparisons; GET
/// /api/me/comparisons/{id}?locale= returns the per-viewer report (differences before similarities,
/// EN/DE localized, no compatibility score and no raw answers). A non-participant gets 403 +
/// access_denied; a still-pending comparison returns a pending marker.
/// </summary>
public sealed class ViewComparisonTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ViewComparisonTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task List_ReturnsTheCallersComparison_WithOtherLabelAndCompleteStatus()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, inviterCookie, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        var body = await GetJson(client, "/api/me/comparisons", inviterCookie);
        var comparisons = body.GetProperty("comparisons").EnumerateArray().ToList();

        var mine = comparisons.Single(c => c.GetProperty("comparisonId").GetGuid() == comparisonId);
        mine.GetProperty("otherLabel").GetString().Should().Be("Sam");
        mine.GetProperty("status").GetString().Should().Be("complete");
    }

    [Fact]
    public async Task Report_IsPerViewer_EachSeesTheOtherPersonsLabel()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, inviterCookie, inviteeCookie) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        var inviterView = await GetJson(client, $"/api/me/comparisons/{comparisonId}", inviterCookie);
        var inviteeView = await GetJson(client, $"/api/me/comparisons/{comparisonId}", inviteeCookie);

        // "You" is always the viewer; the other is named by their label.
        inviterView.GetProperty("otherLabel").GetString().Should().Be("Sam");
        inviteeView.GetProperty("otherLabel").GetString().Should().Be("Alex");
    }

    [Fact]
    public async Task Report_CarriesNoCompatibilityScore_AndNoRawAnswers()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, inviterCookie, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        var report = await GetJson(client, $"/api/me/comparisons/{comparisonId}", inviterCookie);

        // Only the report shape — no score/fit verdict, no answers.
        report.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(["otherLabel", "groups"]);

        var insight = report.GetProperty("groups").EnumerateArray()
            .SelectMany(g => g.GetProperty("insights").EnumerateArray())
            .First();
        insight.EnumerateObject().Select(p => p.Name).Should().BeSubsetOf(
            ["dimensionId", "title", "yourStrength", "theirStrength", "yourText", "theirText", "classification"]);
    }

    [Fact]
    public async Task Report_OrdersDifferencesBeforeSimilarities_WithinEachGroup()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, inviterCookie, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        var report = await GetJson(client, $"/api/me/comparisons/{comparisonId}", inviterCookie);

        foreach (var group in report.GetProperty("groups").EnumerateArray())
        {
            var kinds = group.GetProperty("insights").EnumerateArray()
                .Select(i => i.GetProperty("classification").GetString())
                .ToList();
            var lastDifference = kinds.LastIndexOf("difference");
            var firstSimilarity = kinds.IndexOf("similarity");
            if (lastDifference >= 0 && firstSimilarity >= 0)
                lastDifference.Should().BeLessThan(firstSimilarity);
        }
    }

    [Fact]
    public async Task Report_LocaleDe_LocalizesInsightText_DifferentlyFromEn()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, inviterCookie, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        var en = await GetJson(client, $"/api/me/comparisons/{comparisonId}?locale=en", inviterCookie);
        var de = await GetJson(client, $"/api/me/comparisons/{comparisonId}?locale=de", inviterCookie);

        string? FirstYourText(JsonElement report) => report.GetProperty("groups").EnumerateArray()
            .SelectMany(g => g.GetProperty("insights").EnumerateArray())
            .Select(i => i.TryGetProperty("yourText", out var t) ? t.GetString() : null)
            .FirstOrDefault(t => !string.IsNullOrEmpty(t));

        var enText = FirstYourText(en);
        enText.Should().NotBeNullOrEmpty();
        FirstYourText(de).Should().NotBe(enText);
    }

    [Fact]
    public async Task Report_NonParticipant_Returns403_AndAuditsAccessDenied()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, _, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        // A third, unrelated session.
        var outsiderCookie = await ComparisonTestFlow.SessionForNewResponse(client);
        var response = await ComparisonTestFlow.SendWithCookie(client, HttpMethod.Get, $"/api/me/comparisons/{comparisonId}", outsiderCookie);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await ComparisonTestFlow.AuditExists(Factory, "access_denied", comparisonId)).Should().BeTrue();
    }

    [Fact]
    public async Task Report_NoSession_Returns401()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, _, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        var response = await client.GetAsync($"/api/me/comparisons/{comparisonId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Report_UnknownId_Returns404()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var cookie = await ComparisonTestFlow.SessionForNewResponse(client);

        var response = await ComparisonTestFlow.SendWithCookie(client, HttpMethod.Get, $"/api/me/comparisons/{Guid.NewGuid()}", cookie);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Report_Unavailable_ReturnsUnavailableMarker()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (comparisonId, inviterCookie, _) = await ComparisonTestFlow.CompleteComparison(client, "Alex", "Sam");

        await ComparisonTestFlow.SetUnavailable(Factory, comparisonId);

        var body = await GetJson(client, $"/api/me/comparisons/{comparisonId}", inviterCookie);
        body.GetProperty("state").GetString().Should().Be("unavailable");
    }

    [Fact]
    public async Task Report_StillPending_ReturnsPendingMarker()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (_, comparisonId, inviterCookie) = await ComparisonTestFlow.CreateInvite(client, "Alex"); // no invitee yet

        var response = await ComparisonTestFlow.SendWithCookie(client, HttpMethod.Get, $"/api/me/comparisons/{comparisonId}", inviterCookie);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("state").GetString().Should().Be("pending");
    }

    private static async Task<JsonElement> GetJson(HttpClient client, string url, string cookie)
    {
        var response = await ComparisonTestFlow.SendWithCookie(client, HttpMethod.Get, url, cookie);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
    }
}
