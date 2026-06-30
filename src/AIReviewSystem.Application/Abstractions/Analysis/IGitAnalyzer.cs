using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Analysis;

public interface IGitAnalyzer
{
    Task<(RepositorySnapshot Snapshot, IReadOnlyList<ChangedFile> ChangedFiles)> AnalyzeAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);
}