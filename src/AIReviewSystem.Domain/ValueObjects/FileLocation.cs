namespace AIReviewSystem.Domain.ValueObjects;

public sealed record FileLocation(string FilePath, int? Line = null);