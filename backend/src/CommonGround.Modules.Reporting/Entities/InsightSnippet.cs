namespace CommonGround.Modules.Reporting.Entities;

public sealed class InsightSnippet
{
    public Guid Id { get; init; }
    public string DimensionId { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}
