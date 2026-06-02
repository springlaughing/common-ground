namespace CommonGround.Modules.Reporting.Entities;

public sealed class DimensionGroupMembership
{
    public Guid Id { get; init; }
    public Guid DimensionGroupId { get; init; }
    public string DimensionId { get; init; } = string.Empty;
    public int OrderIndex { get; init; }

    public DimensionGroup DimensionGroup { get; init; } = null!;
}
