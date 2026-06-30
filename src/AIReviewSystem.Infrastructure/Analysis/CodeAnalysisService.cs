using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Domain.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Logging;

namespace AIReviewSystem.Infrastructure.Analysis;

public sealed class CodeAnalysisService
{
    private readonly ILogger<CodeAnalysisService> _logger;

    public CodeAnalysisService(ILogger<CodeAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<StaticFinding>> AnalyzeWorkspaceAsync(
        string workspacePath,
        Guid analysisSessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePath);

        if (!Directory.Exists(workspacePath))
        {
            throw new DirectoryNotFoundException($"Папка анализа не существует: {workspacePath}");
        }

        try
        {
            MSBuildLocator.RegisterDefaults();
        }
        catch
        {
        }

        var solutionFile = FindFile(workspacePath, "*.sln");
        var projectFile = solutionFile is null ? FindFile(workspacePath, "*.csproj") : null;
        if (solutionFile is null && projectFile is null)
        {
            _logger.LogWarning("В рабочей папке {WorkspacePath} не найден solution или project", workspacePath);
            return Array.Empty<StaticFinding>();
        }

        using var workspace = MSBuildWorkspace.Create();
        Solution? solution = null;
        if (solutionFile is not null)
        {
            solution = await workspace.OpenSolutionAsync(solutionFile, cancellationToken: cancellationToken);
        }
        else if (projectFile is not null)
        {
            var project = await workspace.OpenProjectAsync(projectFile, cancellationToken: cancellationToken);
            solution = project.Solution;
        }

        if (solution is null)
        {
            _logger.LogWarning("Не удалось открыть solution/project в {WorkspacePath}", workspacePath);
            return Array.Empty<StaticFinding>();
        }

        var findings = new List<StaticFinding>();
        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation is null)
            {
                continue;
            }

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var filePath = syntaxTree.FilePath;
                if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var diagnostics = compilation.GetDiagnostics()
                    .Where(diagnostic => diagnostic.Location.GetLineSpan().Path.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                    .Where(diagnostic => diagnostic.Severity != DiagnosticSeverity.Hidden)
                    .ToList();

                findings.AddRange(diagnostics.Select(diagnostic => new StaticFinding
                {
                    Id = Guid.NewGuid(),
                    AnalysisSessionId = analysisSessionId,
                    RuleId = diagnostic.Id,
                    Severity = MapSeverity(diagnostic.Severity),
                    Message = diagnostic.GetMessage(),
                    FilePath = Path.GetRelativePath(workspacePath, filePath).Replace('\\', '/'),
                    Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
                    Column = diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1,
                    AnalyzerName = "CodeAnalysisService"
                }));
            }
        }

        return findings;
    }

    private static string? FindFile(string workspacePath, string pattern)
    {
        var fullPath = Path.GetFullPath(workspacePath);
        var candidates = Directory.EnumerateFiles(fullPath, pattern, SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return candidates.FirstOrDefault();
    }

    private static SeverityLevel MapSeverity(DiagnosticSeverity severity)
        => severity switch
        {
            DiagnosticSeverity.Error => SeverityLevel.High,
            DiagnosticSeverity.Warning => SeverityLevel.Medium,
            DiagnosticSeverity.Info => SeverityLevel.Low,
            _ => SeverityLevel.Info
        };
}
