using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CommonGround.Api.Controllers;
using CommonGround.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CommonGround.IntegrationTests;

public sealed class QuestionnaireFlowTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public QuestionnaireFlowTests(IntegrationTestFactory factory) : base(factory) { }

    // ─── GET /api/questionnaire/current ───────────────────────────────────────

    [Fact]
    public async Task GetCurrent_Returns200_WithQuestionsAndAnswerOptions()
    {
        var response = await Client.GetAsync("/api/questionnaire/current");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
        body.GetProperty("versionNumber").GetString().Should().NotBeNullOrWhiteSpace();

        var questions = body.GetProperty("questions");
        questions.GetArrayLength().Should().Be(46);

        var first = questions[0];
        first.GetProperty("sectionIndex").GetInt32().Should().BeInRange(1, 10);
        first.GetProperty("orderIndex").GetInt32().Should().Be(1);
        first.GetProperty("answerOptions").GetArrayLength().Should().Be(4);
    }

    [Fact]
    public async Task GetCurrent_DoesNotReturn_DimensionWeights()
    {
        var response = await Client.GetAsync("/api/questionnaire/current");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var firstOption = body.GetProperty("questions")[0].GetProperty("answerOptions")[0];

        // Answer options must expose exactly: id, text, orderIndex (no dimension data)
        var properties = firstOption.EnumerateObject().Select(p => p.Name).ToList();
        properties.Should().BeEquivalentTo(new[] { "id", "text", "orderIndex" });
    }

    // ─── POST /api/responses ──────────────────────────────────────────────────

    [Fact]
    public async Task Submit_ValidAnswers_Returns201_WithReflectionAndCredentials()
    {
        var questionnaire = await GetActiveQuestionnaire();
        var request = BuildValidRequest(questionnaire);

        var response = await Client.PostAsJsonAsync("/api/responses", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("privateResultLink").GetString().Should().StartWith("/me#");
        body.GetProperty("accessCode").GetString().Should().MatchRegex(@"^[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}$");

        var groups = body.GetProperty("reflection").GetProperty("groups");
        if (groups.GetArrayLength() > 0)
        {
            var group = groups[0];
            group.GetProperty("id").GetString().Should().NotBeNullOrWhiteSpace();
            group.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();

            var insights = group.GetProperty("insights");
            if (insights.GetArrayLength() > 0)
            {
                var insight = insights[0];
                insight.GetProperty("dimensionId").GetString().Should().NotBeNullOrWhiteSpace();
                insight.GetProperty("text").GetString().Should().NotBeNullOrWhiteSpace();
                insight.GetProperty("strength").GetInt32().Should().BeInRange(1, 5);
            }
        }
    }

    [Fact]
    public async Task Submit_MissingQuestions_Returns400_IncompleteAnswers()
    {
        var questionnaire = await GetActiveQuestionnaire();
        var allAnswers = BuildAllAnswers(questionnaire);

        // Submit only the first 10 answers
        var partialRequest = new SubmitResponseRequest(allAnswers.Take(10).ToList());

        var response = await Client.PostAsJsonAsync("/api/responses", partialRequest, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("incomplete_answers");
    }

    [Fact]
    public async Task Submit_DuplicateQuestionIds_Returns400()
    {
        var questionnaire = await GetActiveQuestionnaire();
        var answers = BuildAllAnswers(questionnaire).ToList();

        // Replace the second answer with a duplicate of the first
        answers[1] = answers[0];

        var response = await Client.PostAsJsonAsync("/api/responses",
            new SubmitResponseRequest(answers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("duplicate_question_ids");
    }

    [Fact]
    public async Task Submit_InvalidAnswerOptionId_Returns400()
    {
        var questionnaire = await GetActiveQuestionnaire();
        var answers = BuildAllAnswers(questionnaire).ToList();

        // Replace the primary option of the first answer with a random (invalid) GUID
        var first = answers[0];
        answers[0] = new AnswerRequest(first.QuestionId, Guid.NewGuid(), null);

        var response = await Client.PostAsJsonAsync("/api/responses",
            new SubmitResponseRequest(answers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_answer_option");
    }

    [Fact]
    public async Task Submit_SecondaryOptionSameAsPrimary_Returns400()
    {
        var questionnaire = await GetActiveQuestionnaire();
        var answers = BuildAllAnswers(questionnaire).ToList();

        // Make secondary equal to primary for the first answer
        var first = answers[0];
        answers[0] = new AnswerRequest(first.QuestionId, first.PrimaryAnswerOptionId, first.PrimaryAnswerOptionId);

        var response = await Client.PostAsJsonAsync("/api/responses",
            new SubmitResponseRequest(answers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("duplicate_answer_option");
    }

    [Fact]
    public async Task Submit_SecondaryOptionFromDifferentQuestion_Returns400()
    {
        var questionnaire = await GetActiveQuestionnaire();
        var answers = BuildAllAnswers(questionnaire).ToList();

        // Use an answer option from question 2 as the secondary of question 1
        var q1 = answers[0];
        var q2OptionId = questionnaire.GetProperty("questions")[1]
            .GetProperty("answerOptions")[1]
            .GetProperty("id").GetGuid();

        answers[0] = new AnswerRequest(q1.QuestionId, q1.PrimaryAnswerOptionId, q2OptionId);

        var response = await Client.PostAsJsonAsync("/api/responses",
            new SubmitResponseRequest(answers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("error").GetString().Should().Be("invalid_answer_option");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<JsonElement> GetActiveQuestionnaire()
    {
        var response = await Client.GetAsync("/api/questionnaire/current");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
    }

    private static SubmitResponseRequest BuildValidRequest(JsonElement questionnaire)
        => new(BuildAllAnswers(questionnaire).ToList());

    private static IEnumerable<AnswerRequest> BuildAllAnswers(JsonElement questionnaire)
    {
        foreach (var question in questionnaire.GetProperty("questions").EnumerateArray())
        {
            var questionId = question.GetProperty("id").GetGuid();
            var options = question.GetProperty("answerOptions").EnumerateArray().ToList();
            yield return new AnswerRequest(questionId, options[0].GetProperty("id").GetGuid(), null);
        }
    }
}
