using CommonGround.Modules.Audit.Entities;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Audit.Services;

public sealed class EfAuditLogger : IAuditLogger
{
    private readonly DbContext _db;

    public EfAuditLogger(DbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        string eventType,
        Guid? responseSetId = null,
        Guid? comparisonSessionId = null,
        string? metadata = null,
        CancellationToken ct = default)
    {
        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            ResponseSetId = responseSetId,
            ComparisonSessionId = comparisonSessionId,
            OccurredAt = DateTimeOffset.UtcNow,
            Metadata = metadata,
        };

        _db.Set<AuditEvent>().Add(auditEvent);
        await _db.SaveChangesAsync(ct);
    }
}
