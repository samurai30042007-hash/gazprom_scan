using AIReviewSystem.Application.Abstractions.Analysis;
using AIReviewSystem.Application.Abstractions.Repositories;
using AIReviewSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AIReviewSystem.Infrastructure.Analysis;

public sealed class RoslynStaticAnalysisService : IStaticAnalysisService
{
    private readonly ILogger<RoslynStaticAnalysisService> _logger;
    private readonly ILanguageAnalyzer _languageAnalyzer;
    private readonly IStaticFindingRepository _findingRepository;

    public RoslynStaticAnalysisService(
        ILogger<RoslynStaticAnalysisService> logger,
        ILanguageAnalyzer languageAnalyzer,
        IStaticFindingRepository findingRepository)
    {
        _logger = logger;
        _languageAnalyzer = languageAnalyzer;
        _findingRepository = findingRepository;
    }

    public async Task<IReadOnlyList<StaticFinding>> AnalyzeAsync(
        AnalysisSession analysisSession,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analysisSession);

        if (analysisSession.ChangedFiles is null || analysisSession.ChangedFiles.Count == 0)
        {
            _logger.LogInformation("Нет изменённых файлов для анализа сессии {SessionId}", analysisSession.Id);
            return Array.Empty<StaticFinding>();
        }

        var findings = new List<StaticFinding>();

        foreach (var changedFile in analysisSession.ChangedFiles.Where(item => item.IsCSharp))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileFindings = await _languageAnalyzer.AnalyzeAsync(analysisSession, changedFile, cancellationToken);
            findings.AddRange(fileFindings);
        }

        if (findings.Count > 0)
        {
            await _findingRepository.AddRangeAsync(findings, cancellationToken);
        }

        return findings;
    }
}
