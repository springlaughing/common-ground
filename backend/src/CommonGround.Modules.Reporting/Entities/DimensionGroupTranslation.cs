namespace CommonGround.Modules.Reporting.Entities;

/// <summary>
/// Localized title for a <see cref="DimensionGroup"/> in a non-default locale.
/// English remains the canonical title on <see cref="DimensionGroup.Title"/> (the fallback).
/// </summary>
public sealed class DimensionGroupTranslation
{
    public Guid Id { get; init; }
    public Guid DimensionGroupId { get; init; }
    public string Locale { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}
