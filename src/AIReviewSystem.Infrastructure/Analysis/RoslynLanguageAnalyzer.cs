using System.Collections.Concurrent;
using AIReviewSystem.Application.Abstractions.Analysis;
using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Domain.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Logging;

namespace AIReviewSystem.Infrastructure.Analysis;

public sealed class RoslynLanguageAnalyzer : ILanguageAnalyzer
{
    private static readonly ConcurrentDictionary<string, Solution> SolutionCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<RoslynLanguageAnalyzer> _logger;

    public RoslynLanguageAnalyzer(ILogger<RoslynLanguageAnalyzer> logger)
    {
        _logger = logger;
    }

    public string LanguageName => "C#";

    public bool CanAnalyze(string filePath)
        => filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<StaticFinding>> AnalyzeAsync(
        AnalysisSession analysisSession,
        ChangedFile changedFile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analysisSession);
        ArgumentNullException.ThrowIfNull(changedFile);

        if (!CanAnalyze(changedFile.FilePath))
        {
            return Array.Empty<StaticFinding>();
        }

        try
        {
            var solution = await GetOrCreateSolutionAsync(analysisSession.RepositoryPath, cancellationToken);
            if (solution is null)
            {
                _logger.LogWarning("Не удалось открыть Solution или Project для файла {FilePath}", changedFile.FilePath);
                return Array.Empty<StaticFinding>();
            }

            var project = solution.Projects.FirstOrDefault(item => item.Documents.Any(document =>
                string.Equals(document.FilePath, ResolveFilePath(analysisSession.RepositoryPath, changedFile.FilePath), StringComparison.OrdinalIgnoreCase)));
            if (project is null)
            {
                _logger.LogWarning("Файл {FilePath} не входит в найденный проект или solution", changedFile.FilePath);
                return Array.Empty<StaticFinding>();
            }

            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation is null)
            {
                _logger.LogWarning("Не удалось создать Compilation для {FilePath}", changedFile.FilePath);
                return Array.Empty<StaticFinding>();
            }

            var filePath = ResolveFilePath(analysisSession.RepositoryPath, changedFile.FilePath);
            var syntaxTree = compilation.SyntaxTrees.FirstOrDefault(tree => string.Equals(tree.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            if (syntaxTree is null)
            {
                _logger.LogWarning("Файл {FilePath} отсутствует в синтаксическом дереве компиляции", changedFile.FilePath);
                return Array.Empty<StaticFinding>();
            }

            var diagnostics = compilation.GetDiagnostics()
                .Where(diagnostic => diagnostic.Location.GetLineSpan().Path.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                .Where(diagnostic => diagnostic.Severity != DiagnosticSeverity.Hidden)
                .Where(diagnostic => diagnostic.Id != "CS1701")
                .Where(diagnostic => diagnostic.Id != "CS1702")
                .ToList();

            return diagnostics
                .Select(diagnostic => CreateFinding(analysisSession, changedFile, diagnostic))
                .ToList();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ошибка Roslyn для файла {FilePath}", changedFile.FilePath);
            return Array.Empty<StaticFinding>();
        }
    }

    private async Task<Solution?> GetOrCreateSolutionAsync(string repositoryPath, CancellationToken cancellationToken)
    {
        if (SolutionCache.TryGetValue(repositoryPath, out var cachedSolution))
        {
            return cachedSolution;
        }

        try
        {
            MSBuildLocator.RegisterDefaults();
        }
        catch
        {
        }

        var solutionFile = FindFile(repositoryPath, "*.sln");
        var projectFile = solutionFile is null ? FindFile(repositoryPath, "*.csproj") : null;
        if (solutionFile is null && projectFile is null)
        {
            return null;
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

        if (solution is not null)
        {
            SolutionCache.TryAdd(repositoryPath, solution);
        }

        return solution;
    }

    private static string? FindFile(string repositoryPath, string pattern)
    {
        var fullRepositoryPath = Path.GetFullPath(repositoryPath);
        var candidateFiles = Directory.EnumerateFiles(fullRepositoryPath, pattern, SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return candidateFiles.FirstOrDefault();
    }

    private static string ResolveFilePath(string repositoryPath, string changedFilePath)
        => Path.GetFullPath(Path.Combine(repositoryPath, changedFilePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)));

    private static StaticFinding CreateFinding(
        AnalysisSession analysisSession,
        ChangedFile changedFile,
        Diagnostic diagnostic)
    {
        var lineSpan = diagnostic.Location.GetLineSpan();
        var line = lineSpan.StartLinePosition.Line + 1;
        var column = lineSpan.StartLinePosition.Character + 1;

        return new StaticFinding
        {
            Id = Guid.NewGuid(),
            AnalysisSessionId = analysisSession.Id,
            RuleId = diagnostic.Id,
            Severity = MapSeverity(diagnostic.Severity),
            Message = diagnostic.GetMessage(),
            FilePath = changedFile.FilePath,
            Line = line,
            Column = column,
            AnalyzerName = "RoslynLanguageAnalyzer"
        };
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
