using AIReviewSystem.Domain.Enums;

namespace AIReviewSystem.Domain.Entities;

public sealed class ChangedFile
{
    public Guid Id { get; set; }

    public Guid AnalysisSessionId { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public ChangeType ChangeType { get; set; }

    public int Additions { get; set; }

    public int Deletions { get; set; }

    public bool IsCSharp { get; set; }

    public AnalysisSession? AnalysisSession { get; set; }
}