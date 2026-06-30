using AIReviewSystem.Application.Abstractions.Repositories;
using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIReviewSystem.Infrastructure.Repositories;

public sealed class EfAnalysisSessionRepository : IAnalysisSessionRepository
{
    private readonly ReviewDbContext _dbContext;

    public EfAnalysisSessionRepository(ReviewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AnalysisSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.AnalysisSessions
            .Include(item => item.RepositorySnapshot)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AnalysisSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.AnalysisSessions
            .OrderByDescending(item => item.StartedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return sessions;
    }

    public Task AddAsync(AnalysisSession session, CancellationToken cancellationToken = default)
    {
        return _dbContext.AnalysisSessions.AddAsync(session, cancellationToken).AsTask();
    }

    public Task UpdateAsync(AnalysisSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.AnalysisSessions.Update(session);
        return Task.CompletedTask;
    }
}