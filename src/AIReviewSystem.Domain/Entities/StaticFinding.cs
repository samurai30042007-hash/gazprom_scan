using AIReviewSystem.Domain.Enums;

namespace AIReviewSystem.Domain.Entities;

public sealed class StaticFinding
{
    public Guid Id { get; set; }

    public Guid AnalysisSessionId { get; set; }

    public string RuleId { get; set; } = string.Empty;

    public SeverityLevel Severity { get; set; }

    public string Message { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int? Line { get; set; }

    public int? Column { get; set; }

    public string AnalyzerName { get; set; } = string.Empty;

    public AnalysisSession? AnalysisSession { get; set; }
}