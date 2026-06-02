namespace CommonGround.Modules.Reporting.Entities;

public sealed class DimensionScore
{
    public Guid Id { get; init; }
    public Guid ResponseSetId { get; init; }
    public string DimensionId { get; init; } = string.Empty;
    public decimal RawScore { get; init; }
    public decimal NormalisedScore { get; init; }
}
