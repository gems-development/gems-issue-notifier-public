using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Application.Responses;
using Gems.TechSupport.Application.Responses.Models;
using Gems.TechSupport.Domain.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gems.TechSupport.Infrastructure.Services.Okdesk;

public class OkdeskService(HttpClient httpClient, IOptionsMonitor<OkdeskOptions> options) : IOkdeskService
{
    public async Task<Issue> GetIssueDetailsByIdAsync(
        GetIssueDetailsByIdRequest request,
        CancellationToken cancellationToken)
    {
        var okdeskOptions = options.CurrentValue;

        var uriString = $"{request.IssueId}?api_token={okdeskOptions.ApiToken}";

        var response = (await httpClient.GetFromJsonAsync<IssueResponse>(uriString, cancellationToken))!;

        var issue = response.ToDomainExisting();

        return issue;
    }

    public IAsyncEnumerable<IReadOnlyCollection<Issue>> GetUpdatedIssuesAsync(
        GetUpdatedIssuesRequest request,
        CancellationToken cancellationToken)
    {
        var uri = BuildUpdatedIssuesUri(request);

        return GetAllPages(uri, request.UpdatedSince, request.UpdatedUntil, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Comment>> GetIssueCommentsAsync(
        GetIssueCommentsRequest request,
        CancellationToken cancellationToken)
    {
        var okdeskOptions = options.CurrentValue;

        var uriString = $"{request.IssueId}/comments?api_token={okdeskOptions.ApiToken}";

        var response = (await httpClient.GetFromJsonAsync<List<CommentResponse>>(uriString, cancellationToken))!;

        var comments = response
            .Select(x => x.ToDomain(request.IssueId))
            .OfType<Comment>()
            .ToList();

        return comments!;
    }

    public Task SetIssueStatusAsync(SetIssueStatusRequest request, CancellationToken cancellationToken)
    {
        var okdeskOptions = options.CurrentValue;

        var uriString = $"{request.IssueId}/statuses?api_token={okdeskOptions.ApiToken}";

        return httpClient.PostAsJsonAsync(uriString, request, cancellationToken);
    }

    public Task DeleteIssueAsync(DeleteIssueRequest request, CancellationToken cancellationToken)
    {
        var okdeskOptions = options.CurrentValue;

        var uriString = $"{request.IssueId}?api_token={okdeskOptions.ApiToken}";

        return httpClient.DeleteAsync(uriString, cancellationToken);
    }

    public Task PostCommentAsync(PostIssueCommentRequest request, CancellationToken cancellationToken)
    {
        var okdeskOptions = options.CurrentValue;

        var uriString = $"{request.IssueId}/comments?api_token={okdeskOptions.ApiToken}";

        return httpClient.PostAsJsonAsync(uriString, request, cancellationToken);
    }

    private async IAsyncEnumerable<List<Issue>> GetAllPages(
        string requestUri,
        DateTime since,
        DateTime until,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int pageNumber = 1;
        int issuesResponseCount;

        do
        {
            var uri = requestUri + $"&page[number]={pageNumber}";
            var response = (await httpClient.GetFromJsonAsync<List<IssueResponse>>(uri, cancellationToken))!;
            issuesResponseCount = response.Count;

            if (issuesResponseCount == 0)
            {
                yield break;
            }

            List<Issue> issues = [];

            foreach (var issue in response)
            {
                var createdAt = issue.CreatedAt;

                if (createdAt is not null && issue.IsSkitType is false)
                {
                    if (createdAt >= since && createdAt <= until)
                    {
                        issues.Add(issue.ToDomainNew());
                    }
                    else
                    {
                        issues.Add(issue.ToDomainExisting());
                    }
                }
                else
                {
                    issues.Add(issue.ToDomainExisting());
                }
            }

            yield return issues;
            pageNumber++;
        } while (issuesResponseCount != 0);
    }

    private string BuildUpdatedIssuesUri(GetUpdatedIssuesRequest request)
    {
        var okdeskOptions = options.CurrentValue;

        var uriBuilder = new StringBuilder("list?");
        uriBuilder.AppendJoin('&', $"api_token={okdeskOptions.ApiToken}", $"fields[issue]={okdeskOptions.Fields}",
            $"updated_since={request.UpdatedSince:dd-MM-yyyy HH:mm}", $"updated_until={request.UpdatedUntil:dd-MM-yyyy HH:mm}");

        if (request.PageSize is not null)
        {
            uriBuilder.Append($"&page[size]={request.PageSize}");
        }

        HandleUriFilters(uriBuilder);

        return uriBuilder.ToString();
    }

    private void HandleUriFilters(StringBuilder uriBuilder)
    {
        var okdeskOptions = options.CurrentValue;

        var companyIds = okdeskOptions.FilterByCompanyIds;
        var assigneeIds = okdeskOptions.FilterByAssigneeIds;
        var contactIds = okdeskOptions.FilterByContactIds;
        var statuses = okdeskOptions.FilterByStatutes;

        if (statuses is not null)
        {
            foreach (var status in statuses)
            {
                uriBuilder.Append('&');
                uriBuilder.Append($"status_codes[]={status}");
            }
        }

        if (companyIds is not null)
        {
            foreach (var companyId in companyIds)
            {
                uriBuilder.Append('&');
                uriBuilder.Append($"company_ids[]={companyId}");
            }
        }

        if (assigneeIds is not null)
        {
            foreach (var assigneeId in assigneeIds)
            {
                uriBuilder.Append('&');
                uriBuilder.Append($"assignee_ids[]={assigneeId}");
            }
        }

        if (contactIds is not null)
        {
            foreach (var contactId in contactIds)
            {
                uriBuilder.Append('&');
                uriBuilder.Append($"contact_ids[]={contactId}");
            }
        }
    }
}
