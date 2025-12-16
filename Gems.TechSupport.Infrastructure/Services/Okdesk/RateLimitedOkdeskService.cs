using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using System.Runtime.CompilerServices;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk;

public sealed class RateLimitedOkdeskService(IOkdeskService okdeskService) : IOkdeskService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Task<Issue> GetIssueDetailsByIdAsync(GetIssueDetailsByIdRequest request, CancellationToken cancellationToken)
    {
        return ExecuteWithSemaphore(request, okdeskService.GetIssueDetailsByIdAsync, cancellationToken);
    }

    public async IAsyncEnumerable<IReadOnlyCollection<Issue>> GetUpdatedIssuesAsync(
        GetUpdatedIssuesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var batch in ExecuteWithSemaphore(
            request,
            okdeskService.GetUpdatedIssuesAsync,
            cancellationToken))
        {
            yield return batch;
        }
    }
    public Task<IReadOnlyCollection<Comment>> GetIssueCommentsAsync(GetIssueCommentsRequest request, CancellationToken cancellationToken)
    {
        return ExecuteWithSemaphore(request, okdeskService.GetIssueCommentsAsync, cancellationToken);
    }

    public Task SetIssueStatusAsync(SetIssueStatusRequest request, CancellationToken cancellationToken)
    {
        return ExecuteWithSemaphore(request, okdeskService.SetIssueStatusAsync, cancellationToken);
    }

    public Task DeleteIssueAsync(DeleteIssueRequest request, CancellationToken cancellationToken)
    {
        return ExecuteWithSemaphore(request, okdeskService.DeleteIssueAsync, cancellationToken);
    }

    public Task PostCommentAsync(PostIssueCommentRequest request, CancellationToken cancellationToken)
    {
        return ExecuteWithSemaphore(request, okdeskService.PostCommentAsync ,cancellationToken);
    }

    private async Task ExecuteWithSemaphore<TRequest>(
        TRequest request,
        Func<TRequest, CancellationToken, Task> httpRequest,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await httpRequest(request, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<TResult> ExecuteWithSemaphore<TRequest, TResult>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> httpRequest,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var result = await httpRequest(request, cancellationToken);
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async IAsyncEnumerable<TResult> ExecuteWithSemaphore<TRequest, TResult>(
        TRequest request,
        Func<TRequest, CancellationToken, IAsyncEnumerable<TResult>> httpRequest,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await foreach (var batch in httpRequest(request, cancellationToken))
            {
                yield return batch;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
