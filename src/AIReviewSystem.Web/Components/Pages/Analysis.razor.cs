using AIReviewSystem.Application.Abstractions;
using AIReviewSystem.Application.Abstractions.Analysis;
using AIReviewSystem.Application.Abstractions.Repositories;
using AIReviewSystem.Domain.Entities;
using AIReviewSystem.Domain.Enums;
using AIReviewSystem.Infrastructure.Analysis;
using LibGit2Sharp;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace AIReviewSystem.Web.Components.Pages;

public partial class Analysis : ComponentBase
{
    [Inject] private IGitAnalyzer GitAnalyzer { get; set; } = default!;
    [Inject] private IStaticAnalysisService StaticAnalysisService { get; set; } = default!;
    [Inject] private IAnalysisSessionRepository AnalysisSessionRepository { get; set; } = default!;
    [Inject] private IRepositoryProvider RepositoryProvider { get; set; } = default!;
    [Inject] private IUnitOfWork UnitOfWork { get; set; } = default!;
    [Inject] private CodeAnalysisService CodeAnalysisService { get; set; } = default!;
    [Inject] private ILogger<Analysis> Logger { get; set; } = default!;

    private string repositoryPath = string.Empty;
    private int selectedSourceKind = 0;
    private string? resolvedRepositoryPath;
    private string? errorMessage;
    private string? lastAnalyzedAt;
    private bool isAnalyzing;
    private int totalLinesAdded;
    private int totalLinesDeleted;
    private List<ChangeRow> rows = new();
    private List<StaticFinding> findings = new();
    private IReadOnlyList<IBrowserFile>? uploadedFiles;

    private async Task AnalyzeAsync()
    {
        errorMessage = null;
        resolvedRepositoryPath = null;
        lastAnalyzedAt = null;
        totalLinesAdded = 0;
        totalLinesDeleted = 0;
        rows = new();
        findings = new();

        isAnalyzing = true;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();

        try
        {
            if (selectedSourceKind == (int)RepositorySourceKind.ZipArchive)
            {
                if (uploadedFiles is null || uploadedFiles.Count == 0)
                {
                    throw new InvalidOperationException("Загрузите папку с файлами проекта для анализа.");
                }

                var tempDirectory = Path.Combine(Path.GetTempPath(), "ai-review-system-upload", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDirectory);

                foreach (var file in uploadedFiles)
                {
                    var targetPath = Path.Combine(tempDirectory, file.Name);
                    await using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
                    await using var targetStream = File.Create(targetPath);
                    await stream.CopyToAsync(targetStream);
                }

                var analysisSession = new AnalysisSession
                {
                    Id = Guid.NewGuid(),
                    RepositoryPath = tempDirectory,
                    Status = AnalysisStatus.Running,
                    StartedAt = DateTimeOffset.Now
                };

                await AnalysisSessionRepository.AddAsync(analysisSession);
                await UnitOfWork.SaveChangesAsync();

                findings = (await CodeAnalysisService.AnalyzeWorkspaceAsync(tempDirectory, analysisSession.Id, CancellationToken.None)).ToList();
                rows = findings
                    .Select(item => new ChangeRow(item.FilePath, GetSeverityLabel(item.Severity), 0, 0))
                    .DistinctBy(item => item.Path)
                    .ToList();

                resolvedRepositoryPath = tempDirectory;
                lastAnalyzedAt = DateTimeOffset.Now.ToString("dd.MM.yyyy HH:mm:ss");
                return;
            }

            string inputPath = repositoryPath.Trim();
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                throw new InvalidOperationException("Укажите путь к локальному Git-репозиторию или выберите архив с проектом.");
            }

            var request = new RepositoryRequest(inputPath, (RepositorySourceKind)selectedSourceKind);
            var repositoryCandidate = await RepositoryProvider.ResolveAsync(request, CancellationToken.None);

            var (snapshot, changedFiles) = await GitAnalyzer.AnalyzeAsync(repositoryCandidate, CancellationToken.None);
            var session = new AnalysisSession
            {
                Id = Guid.NewGuid(),
                RepositoryPath = repositoryCandidate,
                Status = AnalysisStatus.Running,
                StartedAt = DateTimeOffset.Now,
                RepositorySnapshot = snapshot
            };

            session.ChangedFiles = changedFiles.ToList();
            await AnalysisSessionRepository.AddAsync(session);
            await UnitOfWork.SaveChangesAsync();

            var csharpFiles = session.ChangedFiles.Where(item => item.IsCSharp).ToList();
            rows = csharpFiles
                .Select(file => new ChangeRow(file.FilePath, GetStatusLabel(ChangeKind.Modified), file.Additions, file.Deletions))
                .ToList();

            totalLinesAdded = rows.Sum(item => item.LinesAdded);
            totalLinesDeleted = rows.Sum(item => item.LinesDeleted);
            findings = (await StaticAnalysisService.AnalyzeAsync(session, CancellationToken.None)).ToList();
            resolvedRepositoryPath = repositoryCandidate;
            lastAnalyzedAt = DateTimeOffset.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }
        catch (Exception exception)
        {
            errorMessage = exception.Message;
            Logger.LogError(exception, "Ошибка анализа репозитория");
        }
        finally
        {
            isAnalyzing = false;
        }
    }

    private void OnInputFileChange(InputFileChangeEventArgs args)
    {
        uploadedFiles = args.GetMultipleFiles();
    }

    private static string GetSourceBadgeLabel()
        => "Upload / local";

    private static string GetStatusLabel(ChangeKind status)
        => status switch
        {
            ChangeKind.Added => "Добавлен",
            ChangeKind.Deleted => "Удалён",
            ChangeKind.Modified => "Изменён",
            ChangeKind.Renamed => "Переименован",
            ChangeKind.Copied => "Скопирован",
            ChangeKind.TypeChanged => "Смена типа",
            ChangeKind.Conflicted => "Конфликт",
            ChangeKind.Untracked => "Неотслеживаемый",
            ChangeKind.Ignored => "Игнорируется",
            _ => status.ToString()
        };

    private static string GetSeverityBadgeClass(SeverityLevel severity)
        => severity switch
        {
            SeverityLevel.Critical => "text-bg-danger",
            SeverityLevel.High => "text-bg-danger",
            SeverityLevel.Medium => "text-bg-warning text-dark",
            SeverityLevel.Low => "text-bg-info text-dark",
            _ => "text-bg-secondary"
        };

    private static string GetSeverityLabel(SeverityLevel severity)
        => severity switch
        {
            SeverityLevel.Critical => "Critical",
            SeverityLevel.High => "High",
            SeverityLevel.Medium => "Medium",
            SeverityLevel.Low => "Low",
            _ => "Info"
        };

    private sealed record ChangeRow(string Path, string Status, int LinesAdded, int LinesDeleted);
}
