using AIReviewSystem.Application.Abstractions.Analysis;
using LibGit2Sharp;

namespace AIReviewSystem.Infrastructure.Analysis;

public sealed class GitRepositoryProvider : IRepositoryProvider
{
    public Task<string> ResolveAsync(RepositoryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.SourceKind switch
        {
            RepositorySourceKind.LocalPath => ResolveLocalPathAsync(request.Source, cancellationToken),
            RepositorySourceKind.LocalFolder => ResolveLocalFolderAsync(request.Source, cancellationToken),
            RepositorySourceKind.ZipArchive => ResolveZipArchiveAsync(request.Source, cancellationToken),
            RepositorySourceKind.GitUrl => ResolveGitUrlAsync(request.Source, cancellationToken),
            _ => throw new NotSupportedException($"Источник репозитория {request.SourceKind} не поддерживается")
        };
    }

    private static Task<string> ResolveLocalPathAsync(string source, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Путь к репозиторию не может быть пустым", nameof(source));
        }

        var fullPath = Path.GetFullPath(source);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Путь не существует: {fullPath}");
        }

        var discoveredRepository = Repository.Discover(fullPath);
        if (string.IsNullOrWhiteSpace(discoveredRepository) || !Repository.IsValid(discoveredRepository))
        {
            throw new InvalidOperationException($"Путь не является Git-репозиторием: {fullPath}");
        }

        return Task.FromResult(discoveredRepository);
    }

    private static Task<string> ResolveLocalFolderAsync(string source, CancellationToken cancellationToken)
    {
        return ResolveLocalPathAsync(source, cancellationToken);
    }

    private static Task<string> ResolveZipArchiveAsync(string source, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("ZIP-архивы пока не поддерживаются в MVP");
    }

    private static Task<string> ResolveGitUrlAsync(string source, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Клонирование по URL пока не поддерживается в MVP");
    }
}
