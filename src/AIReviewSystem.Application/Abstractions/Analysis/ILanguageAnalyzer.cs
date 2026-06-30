using AIReviewSystem.Domain.Entities;

namespace AIReviewSystem.Application.Abstractions.Analysis;

public interface ILanguageAnalyzer
{
    string LanguageName { get; }

    bool CanAnalyze(string filePath);

    Task<IReadOnlyList<StaticFinding>> AnalyzeAsync(
        AnalysisSession analysisSession,
        ChangedFile changedFile,
        CancellationToken cancellationToken = default);
}