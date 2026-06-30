namespace AIReviewSystem.Contracts.Responses;

public sealed record ReportArtifactResponse(
    Guid Id,
    string Format,
    string Location,
    string ContentHash,
    DateTimeOffset CreatedAt);