namespace CommonGround.SharedKernel.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string eventType, Guid? responseSetId = null, Guid? comparisonSessionId = null, string? metadata = null, CancellationToken ct = default);
}
