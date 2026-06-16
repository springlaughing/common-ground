namespace CommonGround.Modules.Reporting.Entities;

/// <summary>
/// Short, neutral, descriptive title shown on a reflection insight (above the strength
/// dots), keyed by dimension. Net-new for the bilingual feature and <b>locale-first</b>:
/// one row per (DimensionId, Locale) for every supported locale, including English.
/// English ("en") is the fallback locale.
/// </summary>
public sealed class DimensionTitle
{
    public Guid Id { get; init; }
    public string DimensionId { get; init; } = string.Empty;
    public string Locale { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}
