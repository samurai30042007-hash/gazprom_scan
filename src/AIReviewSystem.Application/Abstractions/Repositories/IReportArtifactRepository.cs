using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Repositories;

public interface IReportArtifactRepository
{
    Task<ReportArtifact?> GetLatestBySessionIdAsync(Guid analysisSessionId, CancellationToken cancellationToken = default);

    Task AddAsync(ReportArtifact artifact, CancellationToken cancellationToken = default);
}
