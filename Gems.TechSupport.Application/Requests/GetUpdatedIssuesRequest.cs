namespace Gems.TechSupport.Application.Requests;

public record GetUpdatedIssuesRequest(
    DateTime UpdatedSince,
    DateTime UpdatedUntil,
    int? PageSize = null);
