namespace AIReviewSystem.Domain.ValueObjects;

public sealed record CommitRange(string BaseCommit, string TargetCommit);