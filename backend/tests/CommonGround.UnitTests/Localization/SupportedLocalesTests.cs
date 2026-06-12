using CommonGround.SharedKernel.Localization;
using FluentAssertions;

namespace CommonGround.UnitTests.Localization;

public sealed class SupportedLocalesTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("de", "de")]
    [InlineData("EN", "en")]      // case-insensitive, normalised to lower
    [InlineData("De", "de")]
    [InlineData("  de  ", "de")]  // trimmed, then matched
    [InlineData("fr", "en")]      // unsupported -> default
    [InlineData("", "en")]        // empty -> default
    [InlineData(null, "en")]      // missing -> default
    [InlineData("de-DE", "en")]   // region variant is not an exact supported code -> default
    public void Resolve_returns_supported_locale_or_english_default(string? requested, string expected) =>
        SupportedLocales.Resolve(requested).Should().Be(expected);

    [Fact]
    public void Default_is_english() => SupportedLocales.Default.Should().Be("en");

    [Fact]
    public void All_contains_exactly_en_and_de() =>
        SupportedLocales.All.Should().BeEquivalentTo(new[] { "en", "de" });
}
