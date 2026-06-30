using AIReviewSystem.Domain.Enums;

namespace AIReviewSystem.Domain.Entities;

public sealed class AnalysisSession
{
    public Guid Id { get; set; }

    public string RepositoryPath { get; set; } = string.Empty;

    public AnalysisStatus Status { get; set; } = AnalysisStatus.Draft;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? Summary { get; set; }

    public RepositorySnapshot? RepositorySnapshot { get; set; }

    public ICollection<ChangedFile> ChangedFiles { get; set; } = new List<ChangedFile>();

    public ICollection<StaticFinding> StaticFindings { get; set; } = new List<StaticFinding>();

    public ICollection<ReportArtifact> ReportArtifacts { get; set; } = new List<ReportArtifact>();
}