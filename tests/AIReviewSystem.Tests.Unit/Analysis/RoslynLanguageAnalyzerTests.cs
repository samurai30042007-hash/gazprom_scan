using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Domain.Enums;
using AIReviewSystem.Infrastructure.Analysis;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AIReviewSystem.Tests.Unit.Analysis;

public sealed class RoslynLanguageAnalyzerTests
{
    [Fact]
    public async Task AnalyzeAsync_ReturnsDiagnosticsForChangedCSharpFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "ai-review-system-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "SampleApp.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                </Project>
                """);

            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "Program.cs"),
                """
                class Program
                {
                    static void Main()
                    {
                        int value = "not-an-int";
                    }
                }
                """);

            var analyzer = new RoslynLanguageAnalyzer(NullLogger<RoslynLanguageAnalyzer>.Instance);
            var session = new AnalysisSession
            {
                Id = Guid.NewGuid(),
                RepositoryPath = tempDirectory,
                Status = AnalysisStatus.Running
            };

            var changedFile = new ChangedFile
            {
                Id = Guid.NewGuid(),
                AnalysisSessionId = session.Id,
                FilePath = "Program.cs",
                ChangeType = ChangeType.Modified,
                IsCSharp = true
            };

            var findings = await analyzer.AnalyzeAsync(session, changedFile);

            Assert.NotEmpty(findings);
            Assert.Contains(findings, item => item.RuleId == "CS0029");
            Assert.Contains(findings, item => item.FilePath.EndsWith("Program.cs", StringComparison.OrdinalIgnoreCase));
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
