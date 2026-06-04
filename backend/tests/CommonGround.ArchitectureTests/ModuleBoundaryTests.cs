using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace CommonGround.ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    private static readonly string[] AllModules =
    [
        "CommonGround.Modules.Questionnaires",
        "CommonGround.Modules.Responses",
        "CommonGround.Modules.Reporting",
        "CommonGround.Modules.Privacy",
        "CommonGround.Modules.Audit",
        "CommonGround.Modules.Comparisons",
    ];

    [Fact]
    public void SharedKernel_ShouldNot_Reference_AnyModule()
    {
        var result = Types
            .InAssembly(typeof(CommonGround.SharedKernel.Interfaces.IQuestionnaireReader).Assembly)
            .Should()
            .NotHaveDependencyOnAny(AllModules)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: FailureMessage(result));
    }

    [Fact]
    public void Questionnaires_ShouldNot_Reference_OtherModules() =>
        AssertNoCrossModuleDependency(
            typeof(CommonGround.Modules.Questionnaires.QuestionnairesModuleExtensions).Assembly,
            "CommonGround.Modules.Questionnaires");

    [Fact]
    public void Responses_ShouldNot_Reference_OtherModules() =>
        AssertNoCrossModuleDependency(
            typeof(CommonGround.Modules.Responses.ResponsesModuleExtensions).Assembly,
            "CommonGround.Modules.Responses");

    [Fact]
    public void Reporting_ShouldNot_Reference_OtherModules() =>
        AssertNoCrossModuleDependency(
            typeof(CommonGround.Modules.Reporting.ReportingModuleExtensions).Assembly,
            "CommonGround.Modules.Reporting");

    [Fact]
    public void Privacy_ShouldNot_Reference_OtherModules() =>
        AssertNoCrossModuleDependency(
            typeof(CommonGround.Modules.Privacy.PrivacyModuleExtensions).Assembly,
            "CommonGround.Modules.Privacy");

    [Fact]
    public void Audit_ShouldNot_Reference_OtherModules() =>
        AssertNoCrossModuleDependency(
            typeof(CommonGround.Modules.Audit.AuditModuleExtensions).Assembly,
            "CommonGround.Modules.Audit");

    [Fact]
    public void Comparisons_ShouldNot_Reference_OtherModules() =>
        AssertNoCrossModuleDependency(
            typeof(CommonGround.Modules.Comparisons.ComparisonsModuleExtensions).Assembly,
            "CommonGround.Modules.Comparisons");

    private static void AssertNoCrossModuleDependency(Assembly assembly, string self)
    {
        var others = AllModules.Where(m => m != self).ToArray();
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(others)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []);
}
