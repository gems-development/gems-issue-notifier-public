using Gems.TechSupport.Application.Abstractions.Masking;
using Gems.TechSupport.Application.Responses.Models;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Application.Abstractions.Responses;

namespace Gems.TechSupport.Application.Responses;

internal sealed class ResponseToDomainMapper(IMasker masker): IResponseToDomainMapper

{
    public Contact ToDomain(ContactResponse response)
    {
        return new Contact
        {
            Id = response.Id,
            FullName = masker.MaskFullName(response.Name)
        };
    }

    public Assignee? ToDomain(AssigneeResponse response)
    {
        if (response.Id is null || response.Name is null)
        {
            return null;
        }

        return new Assignee
        {
            Id = response.Id.Value,
            FullName = masker.MaskFullName(response.Name)
        };
    }

    public Company ToDomain(CompanyResponse response)
    {
        return new Company
        {
            Id = response.Id,
            CompanyName = response.Name
        };
    }

    public Comment? ToDomain(CommentResponse response, long issueId)
    {
        if (Enum.Parse<CommentAuthorType>(response.Author.Type, true) == CommentAuthorType.Employee)
        {
            return null;
        }

        return new Comment
        {
            Id = response.Id,
            Content = response.Content,
            Public = response.Public,
            CreatedAt = response.PublishedAt.ToUniversalTime(),
            Contact = new Contact
            {
                Id = response.Author.Id,
                FullName = masker.MaskFullName(response.Author.Name)
            }
        };
    }

    public IssuePriority ToDomain(PriorityResponse response)
    {
        if (Enum.TryParse<IssuePriority>(response.Code, true, out var priority))
        {
            return priority;
        }
        else
        {
            return IssuePriority.Undefined;
        }
    }

    public IssueStatus ToDomain(StatusResponse response)
    {
        if (Enum.TryParse<IssueStatus>(response.Code, true, out var status))
        {
            return status;
        }
        else
        {
            return IssueStatus.Undefined;
        }
    }

    public IssueType ToDomain(TypeResponse response)
    {
        if (Enum.TryParse<IssueType>(response.Code.ToString(), true, out var type))
        {
            return type;
        }
        else
        {
            return IssueType.Undefined;
        }
    }

    public Issue ToDomainExisting(IssueResponse response)
    {
        return Issue.CreateExisting(
            id: response.Id,
            title: response.Title,
            description: response.Description,
            priority: response.Priority is null ? null: ToDomain(response.Priority),
            status: response.Status is null ? null : ToDomain(response.Status),
            type: response.Type is null ? null : ToDomain(response.Type),
            createdAt: response.CreatedAt?.ToUniversalTime(),
            updatedAt: response.UpdatedAt?.ToUniversalTime(),
            deadlineAt: response.DeadlineAt?.ToUniversalTime(),
            completedAt: response.CompletedAt?.ToUniversalTime(),
            company: response.Company is null ? null : ToDomain(response.Company),
            contact: response.Contact is null ? null : ToDomain(response.Contact),
            assignee: response.Assignee is null ? null : ToDomain(response.Assignee)
          );
    }

    public Issue ToDomainNew(IssueResponse response)
    {
        return Issue.CreateNew(
            id: response.Id,
            title: response.Title,
            description: response.Description,
            priority: response.Priority is null ? null : ToDomain(response.Priority),
            status: response.Status is null ? null : ToDomain(response.Status),
            type: response.Type is null ? null : ToDomain(response.Type),
            createdAt: response.CreatedAt?.ToUniversalTime(),
            updatedAt: response.UpdatedAt?.ToUniversalTime(),
            deadlineAt: response.DeadlineAt?.ToUniversalTime(),
            completedAt: response.CompletedAt?.ToUniversalTime(),
            company: response.Company is null ? null : ToDomain(response.Company),
            contact: response.Contact is null ? null : ToDomain(response.Contact),
            assignee: response.Assignee is null ? null : ToDomain(response.Assignee)
          );
    }
}
