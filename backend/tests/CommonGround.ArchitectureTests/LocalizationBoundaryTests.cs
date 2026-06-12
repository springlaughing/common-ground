using CommonGround.Modules.Questionnaires.Entities;
using CommonGround.Modules.Reporting.Entities;
using FluentAssertions;

namespace CommonGround.ArchitectureTests;

/// <summary>
/// Pins each net-new translation entity to its owning module, so localization does not
/// quietly leak schema across module boundaries (Constitution IV — Modular Monolith).
/// The blanket cross-module checks in <see cref="ModuleBoundaryTests"/> still apply.
/// </summary>
public sealed class LocalizationBoundaryTests
{
    [Theory]
    [InlineData(typeof(QuestionTranslation), "CommonGround.Modules.Questionnaires")]
    [InlineData(typeof(AnswerOptionTranslation), "CommonGround.Modules.Questionnaires")]
    [InlineData(typeof(InsightSnippetTranslation), "CommonGround.Modules.Reporting")]
    [InlineData(typeof(DimensionGroupTranslation), "CommonGround.Modules.Reporting")]
    [InlineData(typeof(DimensionTitle), "CommonGround.Modules.Reporting")]
    public void Translation_entity_lives_in_its_owning_module(Type entity, string module)
    {
        entity.Assembly.GetName().Name.Should().Be(module);
        entity.Namespace.Should().Be($"{module}.Entities");
    }
}
