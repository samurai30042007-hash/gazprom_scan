using AIReviewSystem.Application.Abstractions;

namespace AIReviewSystem.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly ReviewDbContext _dbContext;

    public EfUnitOfWork(ReviewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}