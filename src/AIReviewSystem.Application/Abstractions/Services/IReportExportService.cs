using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Services;

public interface IReportExportService
{
    Task<ReportArtifact> CreateMarkdownAsync(Guid analysisSessionId, CancellationToken cancellationToken = default);
}