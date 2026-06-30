namespace AIReviewSystem.Domain.Entities;

public sealed class ReportArtifact
{
    public Guid Id { get; set; }

    public Guid AnalysisSessionId { get; set; }

    public string Format { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string ContentHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public AnalysisSession? AnalysisSession { get; set; }
}