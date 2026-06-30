using AIReviewSystem.Application.Abstractions.Analysis;
using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Domain.Enums;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace AIReviewSystem.Infrastructure.Analysis;

public sealed class LibGit2GitAnalyzer : IGitAnalyzer
{
    private readonly ILogger<LibGit2GitAnalyzer> _logger;

    public LibGit2GitAnalyzer(ILogger<LibGit2GitAnalyzer> logger)
    {
        _logger = logger;
    }

    public Task<(RepositorySnapshot Snapshot, IReadOnlyList<ChangedFile> ChangedFiles)> AnalyzeAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        if (!Directory.Exists(repositoryPath))
        {
            throw new DirectoryNotFoundException($"Путь к репозиторию не существует: {repositoryPath}");
        }

        string discoveredRepositoryPath = Repository.Discover(repositoryPath);
        if (string.IsNullOrWhiteSpace(discoveredRepositoryPath) || !Repository.IsValid(discoveredRepositoryPath))
        {
            throw new InvalidOperationException($"Указанный путь не является Git-репозиторием: {repositoryPath}");
        }

        using var repository = new Repository(discoveredRepositoryPath);
        var headCommit = repository.Head.Tip;
        var treeChanges = repository.Diff.Compare<TreeChanges>();
        var patchStats = repository.Diff.Compare<PatchStats>();

        var changedFiles = treeChanges
            .Select(change => CreateChangedFile(change, patchStats, repositoryPath, discoveredRepositoryPath))
            .Where(item => item is not null)
            .Cast<ChangedFile>()
            .ToList();

        var snapshot = new RepositorySnapshot
        {
            Id = Guid.NewGuid(),
            BranchName = repository.Head.FriendlyName,
            BaseCommit = headCommit.Sha,
            TargetCommit = headCommit.Sha,
            DiffMode = "working-tree"
        };

        _logger.LogInformation("Найдено {Count} изменённых файлов в репозитории {Repository}", changedFiles.Count, discoveredRepositoryPath);

        return Task.FromResult<(RepositorySnapshot Snapshot, IReadOnlyList<ChangedFile> ChangedFiles)>((snapshot, changedFiles));
    }

    private static ChangedFile CreateChangedFile(
        TreeEntryChanges change,
        PatchStats patchStats,
        string requestedPath,
        string discoveredRepositoryPath)
    {
        string changedPath = change.Path ?? change.OldPath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(changedPath))
        {
            return null!;
        }

        var stats = patchStats[changedPath];
        var normalizedPath = NormalizePath(changedPath, requestedPath, discoveredRepositoryPath);

        return new ChangedFile
        {
            Id = Guid.NewGuid(),
            FilePath = normalizedPath,
            ChangeType = MapChangeType(change.Status),
            Additions = stats?.LinesAdded ?? 0,
            Deletions = stats?.LinesDeleted ?? 0,
            IsCSharp = normalizedPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string NormalizePath(string filePath, string requestedPath, string discoveredRepositoryPath)
    {
        var fullPath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.GetFullPath(Path.Combine(discoveredRepositoryPath, filePath));

        if (Path.GetFullPath(discoveredRepositoryPath).Equals(Path.GetFullPath(requestedPath), StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetRelativePath(discoveredRepositoryPath, fullPath).Replace('\\', '/');
        }

        return fullPath;
    }

    private static ChangeType MapChangeType(ChangeKind status)
        => status switch
        {
            ChangeKind.Added => ChangeType.Added,
            ChangeKind.Deleted => ChangeType.Deleted,
            ChangeKind.Renamed => ChangeType.Renamed,
            ChangeKind.Copied => ChangeType.Modified,
            ChangeKind.TypeChanged => ChangeType.Modified,
            ChangeKind.Modified => ChangeType.Modified,
            ChangeKind.Untracked => ChangeType.Added,
            ChangeKind.Conflicted => ChangeType.Modified,
            ChangeKind.Ignored => ChangeType.Modified,
            _ => ChangeType.Modified
        };
}
