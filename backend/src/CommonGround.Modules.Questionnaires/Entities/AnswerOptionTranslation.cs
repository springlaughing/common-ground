namespace CommonGround.Modules.Questionnaires.Entities;

/// <summary>
/// Localized text for an <see cref="AnswerOption"/> in a non-default locale.
/// English remains the canonical text on <see cref="AnswerOption.Text"/> (the fallback).
/// The option <see cref="AnswerOption.Id"/> is unchanged across locales, so switching
/// language mid-questionnaire preserves selected answers.
/// </summary>
public sealed class AnswerOptionTranslation
{
    public Guid Id { get; init; }
    public Guid AnswerOptionId { get; init; }
    public string Locale { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
