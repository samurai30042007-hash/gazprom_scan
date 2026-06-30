using AIReviewSystem.Application.Abstractions.Analysis;
using AIReviewSystem.Infrastructure.Analysis;
using LibGit2Sharp;
using Xunit;

namespace AIReviewSystem.Tests.Unit.Analysis;

public sealed class RepositoryProviderTests
{
    [Fact]
    public async Task ResolveAsync_ForLocalRepository_ReturnsExistingRepositoryPath()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "ai-review-system-repo-provider", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Repository.Init(tempDirectory);
            var provider = new GitRepositoryProvider();

            var resolvedPath = await provider.ResolveAsync(new RepositoryRequest(tempDirectory, RepositorySourceKind.LocalPath));

            Assert.True(Directory.Exists(resolvedPath));
            Assert.True(Repository.IsValid(resolvedPath));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
