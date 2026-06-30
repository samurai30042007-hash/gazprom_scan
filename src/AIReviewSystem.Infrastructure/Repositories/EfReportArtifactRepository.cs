using AIReviewSystem.Application.Abstractions.Repositories;
using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIReviewSystem.Infrastructure.Repositories;

public sealed class EfReportArtifactRepository : IReportArtifactRepository
{
    private readonly ReviewDbContext _dbContext;

    public EfReportArtifactRepository(ReviewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReportArtifact?> GetLatestBySessionIdAsync(Guid analysisSessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReportArtifacts
            .Where(item => item.AnalysisSessionId == analysisSessionId)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAsync(ReportArtifact artifact, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReportArtifacts.AddAsync(artifact, cancellationToken).AsTask();
    }
}