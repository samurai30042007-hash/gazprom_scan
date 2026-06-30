using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Analysis;

public interface IStaticAnalysisService
{
    Task<IReadOnlyList<StaticFinding>> AnalyzeAsync(
        AnalysisSession analysisSession,
        CancellationToken cancellationToken = default);
}