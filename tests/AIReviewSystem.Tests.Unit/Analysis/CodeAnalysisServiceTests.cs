using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Domain.Enums;
using AIReviewSystem.Infrastructure.Analysis;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AIReviewSystem.Tests.Unit.Analysis;

public sealed class CodeAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeWorkspaceAsync_ReturnsFindingsForUploadedProject()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "ai-review-system-code-analysis", Guid.NewGuid().ToString("N"));
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

            var service = new CodeAnalysisService(NullLogger<CodeAnalysisService>.Instance);
            var findings = await service.AnalyzeWorkspaceAsync(tempDirectory, Guid.NewGuid(), CancellationToken.None);

            Assert.NotEmpty(findings);
            Assert.Contains(findings, item => item.RuleId == "CS0029");
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
