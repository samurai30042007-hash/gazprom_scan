namespace AIReviewSystem.Application.Abstractions.Analysis;

public enum RepositorySourceKind
{
    LocalPath = 0,
    LocalFolder = 1,
    ZipArchive = 2,
    GitUrl = 3
}

public sealed record RepositoryRequest(
    string Source,
    RepositorySourceKind SourceKind);

public interface IRepositoryProvider
{
    Task<string> ResolveAsync(RepositoryRequest request, CancellationToken cancellationToken = default);
}
