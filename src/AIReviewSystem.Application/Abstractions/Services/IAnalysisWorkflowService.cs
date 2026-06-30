namespace AIReviewSystem.Application.Abstractions.Services;

public interface IAnalysisWorkflowService
{
    Task StartAsync(Guid analysisSessionId, CancellationToken cancellationToken = default);
}