namespace CommonGround.SharedKernel.Interfaces;

public interface IResponseReader
{
    Task<ResponseSetDto?> GetByIdAsync(Guid responseSetId, CancellationToken ct = default);
}

public record ResponseSetDto(
    Guid Id,
    Guid QuestionnaireVersionId,
    bool IsDeleted,
    bool HasAccessCode);
