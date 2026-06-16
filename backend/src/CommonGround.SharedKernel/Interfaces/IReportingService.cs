namespace CommonGround.SharedKernel.Interfaces;

public interface IReportingService
{
    Task<ReflectionDto> AssembleReflectionAsync(Guid responseSetId, string locale, CancellationToken ct = default);
}

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
