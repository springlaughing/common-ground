namespace CommonGround.SharedKernel.Interfaces;

public interface IReportingService
{
    Task<ReflectionDto> AssembleReflectionAsync(Guid responseSetId, string locale, CancellationToken ct = default);

    /// <summary>
    /// The localized per-dimension building blocks the comparison assembler combines for two
    /// people. Unlike <see cref="AssembleReflectionAsync"/>, this returns the *full* group/dimension
    /// structure (shared across responses on a version) so a dimension where one side scores below
    /// threshold still appears — with a null strength and omitted text — letting the engine surface
    /// one-sided differences. Insight text/titles follow the same locale-first English fallback.
    /// </summary>
    Task<ComparisonSourceDto> GetComparisonSourceAsync(Guid responseSetId, string locale, CancellationToken ct = default);
}

public record ComparisonSourceDto(IReadOnlyList<ComparisonSourceGroupDto> Groups);

public record ComparisonSourceGroupDto(
    string Id,
    string Title,
    IReadOnlyList<ComparisonSourceDimensionDto> Dimensions);

public record ComparisonSourceDimensionDto(
    string DimensionId,
    string Title,
    int? Strength,
    string? Text);

public record ReflectionDto(IReadOnlyList<ReflectionGroupDto> Groups);

public record ReflectionGroupDto(
    string Id,
    string Title,
    IReadOnlyList<InsightDto> Insights);

public record InsightDto(
    string DimensionId,
    string Title,
    string Text,
    int Strength);
