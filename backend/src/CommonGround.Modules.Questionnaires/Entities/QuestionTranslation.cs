namespace CommonGround.Modules.Questionnaires.Entities;

/// <summary>
/// Localized text for a <see cref="Question"/> in a non-default locale.
/// English remains the canonical text on <see cref="Question.Text"/> (the fallback).
/// </summary>
public sealed class QuestionTranslation
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string Locale { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
