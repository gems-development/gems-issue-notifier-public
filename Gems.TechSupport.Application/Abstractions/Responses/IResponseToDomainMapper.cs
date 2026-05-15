using Gems.TechSupport.Application.Responses.Models;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;

namespace Gems.TechSupport.Application.Abstractions.Responses;
public interface IResponseToDomainMapper
{
    Contact ToDomain(ContactResponse response);

    Assignee? ToDomain(AssigneeResponse response);

    Company ToDomain(CompanyResponse response);

    Comment? ToDomain(CommentResponse response, long issueId);

    IssuePriority ToDomain(PriorityResponse response);

    IssueStatus ToDomain(StatusResponse response);

    IssueType ToDomain(TypeResponse response);

    Issue ToDomainExisting(IssueResponse response);

    Issue ToDomainNew(IssueResponse response);
}
