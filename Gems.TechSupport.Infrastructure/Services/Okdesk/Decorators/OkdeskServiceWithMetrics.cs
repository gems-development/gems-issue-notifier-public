using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Infrastructure.Metrics;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk.Decorators;

public class OkdeskServiceWithMetrics(IOkdeskService okdeskService, ProcessedPostCommentsMetrics metrics) : IOkdeskService
{
    public Task DeleteIssueAsync(DeleteIssueRequest request, CancellationToken cancellationToken)
        => okdeskService.DeleteIssueAsync(request, cancellationToken);

    public Task<IReadOnlyCollection<Comment>> GetIssueCommentsAsync(GetIssueCommentsRequest request, CancellationToken cancellationToken)
        => okdeskService.GetIssueCommentsAsync(request, cancellationToken);

    public Task<Issue> GetIssueDetailsByIdAsync(GetIssueDetailsByIdRequest request, CancellationToken cancellationToken)
        => okdeskService.GetIssueDetailsByIdAsync(request, cancellationToken);

    public IAsyncEnumerable<IReadOnlyCollection<Issue>> GetUpdatedIssuesAsync(GetUpdatedIssuesRequest request, CancellationToken cancellationToken)
        => okdeskService.GetUpdatedIssuesAsync(request, cancellationToken);

    public async Task PostCommentAsync(PostIssueCommentRequest request, CancellationToken cancellationToken)
    {
        await okdeskService.PostCommentAsync(request, cancellationToken);
        metrics.RecordPostCommentsProcessedSuccessfully(request.GetType().Name);
    }

    public Task SetIssueAutoCompletedStatusAsync(SetIssueAutoCompletedStatusRequest request, CancellationToken cancellationToken)
        => okdeskService.SetIssueAutoCompletedStatusAsync(request, cancellationToken);

    public Task SetIssueStatusAsync(SetIssueStatusRequest request, CancellationToken cancellationToken)
        => okdeskService.SetIssueStatusAsync(request, cancellationToken);
}