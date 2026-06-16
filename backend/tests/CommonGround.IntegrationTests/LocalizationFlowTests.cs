using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.Api.Controllers;
using CommonGround.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CommonGround.IntegrationTests;

/// <summary>
/// User Story 1 (MVP): take the questionnaire and read the reflection in the chosen
/// language. Locale affects display text only — IDs, order, and scoring are invariant
/// (SC-003). English is the field-level fallback.
/// </summary>
public sealed class LocalizationFlowTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // First German question, verbatim from questionary_german.md (Section 1, Q1).
    private const string GermanFirstQuestion =
        "Wenn du mit jemandem neu zusammenarbeitest, was hilft dir, gut ins gemeinsame Arbeiten zu kommen?";

    public LocalizationFlowTests(IntegrationTestFactory factory) : base(factory) { }

    // ─── T016 — GET /api/questionnaire/current?locale=de ──────────────────────

    [Fact]
    public async Task GetCurrent_LocaleDe_ReturnsGermanText_SameIdsAndOrderAsEnglish()
    {
        var en = await GetQuestionnaire("en");
        var de = await GetQuestionnaire("de");

        var enQuestions = en.GetProperty("questions").EnumerateArray().ToList();
        var deQuestions = de.GetProperty("questions").EnumerateArray().ToList();

        deQuestions.Should().HaveCount(enQuestions.Count);

        // The first question is German, verbatim.
        deQuestions[0].GetProperty("text").GetString().Should().Be(GermanFirstQuestion);

        for (var i = 0; i < enQuestions.Count; i++)
        {
            var enQ = enQuestions[i];
            var deQ = deQuestions[i];

            // Same identity and ordering across locales.
            deQ.GetProperty("id").GetGuid().Should().Be(enQ.GetProperty("id").GetGuid());
            deQ.GetProperty("orderIndex").GetInt32().Should().Be(enQ.GetProperty("orderIndex").GetInt32());
            deQ.GetProperty("sectionIndex").GetInt32().Should().Be(enQ.GetProperty("sectionIndex").GetInt32());

            // Localized: the German text differs from the English text.
            deQ.GetProperty("text").GetString().Should().NotBe(enQ.GetProperty("text").GetString());

            var enOptions = enQ.GetProperty("answerOptions").EnumerateArray().ToList();
            var deOptions = deQ.GetProperty("answerOptions").EnumerateArray().ToList();
            deOptions.Should().HaveCount(enOptions.Count);

            for (var j = 0; j < enOptions.Count; j++)
            {
                deOptions[j].GetProperty("id").GetGuid().Should().Be(enOptions[j].GetProperty("id").GetGuid());
                deOptions[j].GetProperty("orderIndex").GetInt32().Should().Be(enOptions[j].GetProperty("orderIndex").GetInt32());
                deOptions[j].GetProperty("text").GetString().Should().NotBe(enOptions[j].GetProperty("text").GetString());
            }
        }
    }

    // ─── T017 — POST /api/responses?locale=de ─────────────────────────────────

    [Fact]
    public async Task Submit_LocaleDe_ReturnsGermanReflection_SameInsightsOrderAndStrengthAsEnglish()
    {
        var questionnaire = await GetQuestionnaire("en");
        var request = BuildAllPrimaryAnswers(questionnaire);

        var enReflection = await SubmitAndGetReflection(request, "en");
        var deReflection = await SubmitAndGetReflection(request, "de");

        // SC-003: identical answers ⇒ identical insight selection, order, and strength
        // regardless of locale.
        var enShape = Shape(enReflection);
        var deShape = Shape(deReflection);
        deShape.Should().BeEquivalentTo(enShape, o => o.WithStrictOrdering());

        // There is something to assert against (all-primary answers qualify insights).
        enShape.Should().NotBeEmpty();

        // Localized: for every group and insight, the German display text differs from
        // the English text for the same id.
        var enGroups = enReflection.GetProperty("groups").EnumerateArray().ToList();
        var deGroups = deReflection.GetProperty("groups").EnumerateArray().ToList();

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

    private async Task<JsonElement> GetQuestionnaire(string locale)
    {
        var response = await Client.GetAsync($"/api/questionnaire/current?locale={locale}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
    }

    private async Task<JsonElement> SubmitAndGetReflection(SubmitResponseRequest request, string locale)
    {
        var response = await Client.PostAsJsonAsync($"/api/responses?locale={locale}", request, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("reflection");
    }

    private static SubmitResponseRequest BuildAllPrimaryAnswers(JsonElement questionnaire)
    {
        var answers = new List<AnswerRequest>();
        foreach (var question in questionnaire.GetProperty("questions").EnumerateArray())
        {
            var questionId = question.GetProperty("id").GetGuid();
            var firstOption = question.GetProperty("answerOptions")[0].GetProperty("id").GetGuid();
            answers.Add(new AnswerRequest(questionId, firstOption, null));
        }
        return new SubmitResponseRequest(answers);
    }

    // A locale-invariant fingerprint of the reflection: ordered (groupId, [(dimensionId, strength)]).
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
