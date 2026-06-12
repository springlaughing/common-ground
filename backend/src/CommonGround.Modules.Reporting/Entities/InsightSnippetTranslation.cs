namespace CommonGround.Modules.Reporting.Entities;

/// <summary>
/// Localized text for an <see cref="InsightSnippet"/> in a non-default locale.
/// English remains the canonical text on <see cref="InsightSnippet.Text"/> (the fallback).
/// </summary>
public sealed class InsightSnippetTranslation
{
    public Guid Id { get; init; }
    public Guid InsightSnippetId { get; init; }
    public string Locale { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
