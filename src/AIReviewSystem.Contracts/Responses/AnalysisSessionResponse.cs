using AIReviewSystem.Contracts.Common;

namespace AIReviewSystem.Contracts.Responses;

public sealed record AnalysisSessionResponse(
    Guid Id,
    string RepositoryPath,
    AnalysisStatusDto Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? Summary);