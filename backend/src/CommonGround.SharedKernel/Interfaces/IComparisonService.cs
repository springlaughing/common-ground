namespace CommonGround.SharedKernel.Interfaces;

/// <summary>
/// Cross-module contract for the one-to-one comparison flow, consumed by the API controllers.
/// The comparison report is computed on read (nothing about it is persisted) and is always
/// returned from the *caller's* perspective. Implemented by the Comparisons module.
/// </summary>
public interface IComparisonService
{
    /// <summary>Inviter creates a single-use, time-limited invite for their own response.</summary>
    Task<CreateInviteResultDto> CreateInviteAsync(
        Guid inviterResponseSetId, string inviterLabel, CancellationToken ct = default);

    /// <summary>Returns the public face of an invite (for the consent screen) without consuming it.</summary>
    Task<InviteValidationDto?> ValidateInviteAsync(string token, CancellationToken ct = default);

    /// <summary>Consumes the invite single-use, attaches the invitee's response + label, and
    /// triggers generation when both responses exist on the same version.</summary>
    Task<JoinComparisonResultDto> JoinAsync(JoinComparisonRequest request, CancellationToken ct = default);

    /// <summary>Lists every comparison the caller's response is part of (the /me hub).</summary>
    Task<IReadOnlyList<ComparisonSummaryDto>> ListComparisonsAsync(
        Guid responseSetId, CancellationToken ct = default);

    /// <summary>The report for one comparison from <paramref name="viewerResponseSetId"/>'s perspective,
    /// in <paramref name="locale"/>. Null when the caller's response is not a participant.</summary>
    Task<ComparisonReportDto?> GetReportAsync(
        Guid comparisonId, Guid viewerResponseSetId, string locale, CancellationToken ct = default);
}

/// <summary>Where a comparison sits: awaiting the invitee, ready, or a dependency is missing.</summary>
public enum ComparisonState
{
    Pending,
    Complete,
    Unavailable,
}

/// <summary>How a dimension reads across the two people: aligned, or a meaningful difference.</summary>
public enum ComparisonClassification
{
    Difference,
    Similarity,
}

public record CreateInviteResultDto(
    Guid ComparisonId,
    string InviteToken,
    DateTimeOffset ExpiresAt,
    ComparisonState Status);

public record InviteValidationDto(
    string InviterLabel,
    string QuestionnaireVersion);

public record ComparisonAnswerDto(
    Guid QuestionId,
    Guid PrimaryAnswerOptionId,
    Guid? SecondaryAnswerOptionId);

public record JoinComparisonRequest(
    string Token,
    bool Consent,
    string InviteeLabel,
    IReadOnlyList<ComparisonAnswerDto> Answers);

public record JoinComparisonResultDto(
    Guid ComparisonId,
    string PrivateResultLink,
    string AccessCode);

public record ComparisonSummaryDto(
    Guid ComparisonId,
    string OtherLabel,
    ComparisonState Status,
    DateTimeOffset CreatedAt);

public record ComparisonReportDto(
    string OtherLabel,
    string Summary,
    IReadOnlyList<ComparisonGroupDto> Groups);

public record ComparisonGroupDto(
    string Id,
    string Title,
    IReadOnlyList<ComparisonInsightDto> Insights);

public record ComparisonInsightDto(
    string DimensionId,
    string Title,
    int? YourStrength,
    int? TheirStrength,
    string? YourText,
    string? TheirText,
    ComparisonClassification Classification);
