namespace AIReviewSystem.Domain.Entities;

public sealed class RepositorySnapshot
{
    public Guid Id { get; set; }

    public Guid AnalysisSessionId { get; set; }

    public string? BranchName { get; set; }

    public string BaseCommit { get; set; } = string.Empty;

    public string TargetCommit { get; set; } = string.Empty;

    public string DiffMode { get; set; } = string.Empty;

    public AnalysisSession? AnalysisSession { get; set; }
}