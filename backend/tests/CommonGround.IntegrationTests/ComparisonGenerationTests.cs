using System.Net;
using CommonGround.IntegrationTests.Infrastructure;
using CommonGround.Modules.Comparisons.Entities;
using FluentAssertions;

namespace CommonGround.IntegrationTests;

/// <summary>
/// T031 (US3) — once both responses exist on the same version (i.e. the invitee joins), the
/// comparison generates automatically: the session becomes Complete and comparison_generated is
/// audited exactly once. It does not generate before the second response exists, and a version
/// mismatch is guarded (no generation).
/// </summary>
public sealed class ComparisonGenerationTests : IntegrationTestBase
{
    public ComparisonGenerationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task AfterBothResponsesExist_SessionBecomesComplete_AndGeneratedAuditedOnce()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        (await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers)).EnsureSuccessStatusCode();

        (await ComparisonTestFlow.SessionStatus(Factory, comparisonId)).Should().Be(ComparisonStatus.Complete);
        // Deterministic + once: generation fires a single comparison_generated event.
        (await ComparisonTestFlow.AuditCount(Factory, "comparison_generated", comparisonId)).Should().Be(1);
    }

    [Fact]
    public async Task BeforeInviteeJoins_SessionStaysPending_AndNotGenerated()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (_, comparisonId, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");

        (await ComparisonTestFlow.SessionStatus(Factory, comparisonId)).Should().Be(ComparisonStatus.Pending);
        (await ComparisonTestFlow.AuditCount(Factory, "comparison_generated", comparisonId)).Should().Be(0);
    }

    [Fact]
    public async Task VersionMismatch_DoesNotGenerate()
    {
        var client = ComparisonTestFlow.NewClient(Factory);
        var (token, comparisonId, _) = await ComparisonTestFlow.CreateInvite(client, "Alex");
        var answers = await ComparisonTestFlow.ValidAnswers(client);

        await ComparisonTestFlow.RepinComparisonVersion(Factory, comparisonId, Guid.NewGuid());

        var response = await ComparisonTestFlow.Join(client, token, consent: true, "Sam", answers);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // version mismatch rejected at join
        (await ComparisonTestFlow.SessionStatus(Factory, comparisonId)).Should().Be(ComparisonStatus.Pending);
        (await ComparisonTestFlow.AuditCount(Factory, "comparison_generated", comparisonId)).Should().Be(0);
    }
}
