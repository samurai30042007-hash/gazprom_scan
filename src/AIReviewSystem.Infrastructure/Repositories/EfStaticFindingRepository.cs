using AIReviewSystem.Application.Abstractions.Repositories;
using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIReviewSystem.Infrastructure.Repositories;

public sealed class EfStaticFindingRepository : IStaticFindingRepository
{
    private readonly ReviewDbContext _dbContext;

    public EfStaticFindingRepository(ReviewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StaticFinding>> GetBySessionIdAsync(Guid analysisSessionId, CancellationToken cancellationToken = default)
    {
        var findings = await _dbContext.StaticFindings
            .Where(item => item.AnalysisSessionId == analysisSessionId)
            .ToListAsync(cancellationToken);

        return findings;
    }

    public Task AddRangeAsync(IEnumerable<StaticFinding> findings, CancellationToken cancellationToken = default)
    {
        return _dbContext.StaticFindings.AddRangeAsync(findings, cancellationToken);
    }
}