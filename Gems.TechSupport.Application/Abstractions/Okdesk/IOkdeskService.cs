using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Models;

namespace Gems.TechSupport.Application.Abstractions.Okdesk;

public interface IOkdeskService
{
    IAsyncEnumerable<IReadOnlyCollection<Issue>> GetUpdatedIssuesAsync(GetUpdatedIssuesRequest request, CancellationToken cancellationToken);
    Task<Issue> GetIssueDetailsByIdAsync(GetIssueDetailsByIdRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Comment>> GetIssueCommentsAsync(GetIssueCommentsRequest request, CancellationToken cancellationToken);
    Task SetIssueStatusAsync(SetIssueStatusRequest request, CancellationToken cancellationToken);
    Task DeleteIssueAsync(DeleteIssueRequest request, CancellationToken cancellationToken);
    Task PostCommentAsync(PostIssueCommentRequest request, CancellationToken cancellationToken);
}
