namespace CommonGround.Modules.Reporting.Entities;

public sealed class DimensionGroup
{
    public Guid Id { get; init; }
    public string GroupId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int OrderIndex { get; init; }

    public ICollection<DimensionGroupMembership> Memberships { get; init; } = [];
}
