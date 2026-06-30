using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Repositories;

public interface IStaticFindingRepository
{
    Task<IReadOnlyList<StaticFinding>> GetBySessionIdAsync(Guid analysisSessionId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<StaticFinding> findings, CancellationToken cancellationToken = default);
}