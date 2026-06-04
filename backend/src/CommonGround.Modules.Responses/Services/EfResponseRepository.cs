using CommonGround.Modules.Responses.Entities;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonGround.Modules.Responses.Services;

internal sealed class EfResponseRepository : IResponseRepository, IResponseReader
{
    private readonly DbContext _db;

    public EfResponseRepository(DbContext db) => _db = db;

    public async Task<ResponseSet> CreateAsync(
        Guid questionnaireVersionId,
        string privateResultTokenHash,
        string accessCodeHash,
        IReadOnlyList<AnswerInput> answers,
        CancellationToken ct = default)
    {
        var id = Guid.NewGuid();

        _db.Set<ResponseSet>().Add(new ResponseSet
        {
            Id = id,
            QuestionnaireVersionId = questionnaireVersionId,
            PrivateResultTokenHash = privateResultTokenHash,
            AccessCodeHash = accessCodeHash,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        foreach (var a in answers)
        {
            _db.Set<Answer>().Add(new Answer
            {
                Id = Guid.NewGuid(),
                ResponseSetId = id,
                QuestionId = a.QuestionId,
                PrimaryAnswerOptionId = a.PrimaryAnswerOptionId,
                SecondaryAnswerOptionId = a.SecondaryAnswerOptionId,
            });
        }

        await _db.SaveChangesAsync(ct);

        return await _db.Set<ResponseSet>()
            .AsNoTracking()
            .FirstAsync(r => r.Id == id, ct);
    }

    public async Task<ResponseSetDto?> GetByIdAsync(Guid responseSetId, CancellationToken ct = default)
    {
        var rs = await _db.Set<ResponseSet>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == responseSetId, ct);

        if (rs is null)
            return null;

        return new ResponseSetDto(
            rs.Id,
            rs.QuestionnaireVersionId,
            rs.IsDeleted,
            !string.IsNullOrEmpty(rs.AccessCodeHash));
    }

    public async Task<ResponseSetDto?> GetByTokenHashAsync(string privateResultTokenHash, CancellationToken ct = default)
    {
        var rs = await _db.Set<ResponseSet>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PrivateResultTokenHash == privateResultTokenHash, ct);

        if (rs is null)
            return null;

        return new ResponseSetDto(
            rs.Id,
            rs.QuestionnaireVersionId,
            rs.IsDeleted,
            !string.IsNullOrEmpty(rs.AccessCodeHash));
    }
}
