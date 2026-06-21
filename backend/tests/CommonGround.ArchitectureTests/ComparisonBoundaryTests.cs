using System.Reflection;
using CommonGround.Modules.Comparisons;
using FluentAssertions;
using NetArchTest.Rules;

namespace CommonGround.ArchitectureTests;

/// <summary>
/// Comparison-specific boundary rules (Principles I + IV): the Comparisons module reaches other
/// modules only through SharedKernel interfaces, and never holds raw answers — its reports are
/// computed from per-dimension scores and localized snippets, never from what anyone actually answered.
/// </summary>
public sealed class ComparisonBoundaryTests
{
    private static readonly Assembly ComparisonsAssembly =
        typeof(ComparisonsModuleExtensions).Assembly;

    [Fact]
    public void Comparisons_ReachesReportingResponsesQuestionnaires_OnlyViaSharedKernel()
    {
        var result = Types.InAssembly(ComparisonsAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "CommonGround.Modules.Reporting",
                "CommonGround.Modules.Responses",
                "CommonGround.Modules.Questionnaires")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "comparison logic must depend on those modules only through SharedKernel interfaces; offenders: "
                + Offenders(result));
    }

    [Fact]
    public void Comparisons_HoldsNoRawAnswerTypes()
    {
        var result = Types.InAssembly(ComparisonsAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "CommonGround.Modules.Responses.Entities.Answer",
                "CommonGround.Modules.Questionnaires.Entities.AnswerOption")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "raw answers must never enter the comparison module (Principle I); offenders: "
                + Offenders(result));
    }

    private static string Offenders(TestResult result) =>
        string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []);
}
