using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Repositories;

public interface IAnalysisSessionRepository
{
    Task<AnalysisSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalysisSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default);

    Task AddAsync(AnalysisSession session, CancellationToken cancellationToken = default);

    Task UpdateAsync(AnalysisSession session, CancellationToken cancellationToken = default);
}