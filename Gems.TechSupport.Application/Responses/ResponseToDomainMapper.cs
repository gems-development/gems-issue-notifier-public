using Gems.TechSupport.Application.Responses.Models;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;

namespace Gems.TechSupport.Application.Responses;

public static class ResponseToDomainMapper
{
    public static Contact ToDomain(this ContactResponse response)
    {
        return new Contact
        {
            Id = response.Id,
            FullName = response.Name
        };
    }

    public static Assignee? ToDomain(this AssigneeResponse response)
    {
        if (response.Id is null || response.Name is null)
        {
            return null;
        }

        return new Assignee
        {
            Id = response.Id.Value,
            FullName = response.Name,
        };
    }

    public static Company ToDomain(this CompanyResponse response)
    {
        return new Company
        {
            Id = response.Id,
            CompanyName = response.Name
        };
    }

    public static Comment? ToDomain(this CommentResponse response, long issueId)
    {
        if (Enum.Parse<CommentAuthorType>(response.Author.Type, true) == CommentAuthorType.Employee)
        {
            return null;
        }

        return new Comment
        {
            Id = response.Id,
            Content = response.Content,
            CreatedAt = response.PublishedAt.ToUniversalTime(),
            Contact = new Contact
            {
                Id = response.Author.Id,
                FullName = response.Author.Name
            }
        };
    }

    public static IssuePriority ToDomain(this PriorityResponse response)
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

    public static IssueStatus ToDomain(this StatusResponse response)
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

    public static IssueType ToDomain(this TypeResponse response)
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

    public static Issue ToDomainExisting(this IssueResponse response)
    {
        return Issue.CreateExisting(
            id: response.Id,
            title: response.Title,
            description: response.Description,
            priority: response.Priority?.ToDomain(),
            status: response.Status?.ToDomain(),
            type: response.Type?.ToDomain(),
            createdAt: response.CreatedAt?.ToUniversalTime(),
            updatedAt: response.UpdatedAt?.ToUniversalTime(),
            deadlineAt: response.DeadlineAt?.ToUniversalTime(),
            completedAt: response.CompletedAt?.ToUniversalTime(),
            company: response.Company?.ToDomain(),
            contact: response.Contact?.ToDomain(),
            assignee: response.Assignee?.ToDomain()
          );
    }

    public static Issue ToDomainNew(this IssueResponse response)
    {
        return Issue.CreateNew(
            id: response.Id,
            title: response.Title,
            description: response.Description,
            priority: response.Priority?.ToDomain(),
            status: response.Status?.ToDomain(),
            type: response.Type?.ToDomain(),
            createdAt: response.CreatedAt?.ToUniversalTime(),
            updatedAt: response.UpdatedAt?.ToUniversalTime(),
            deadlineAt: response.DeadlineAt?.ToUniversalTime(),
            completedAt: response.CompletedAt?.ToUniversalTime(),
            company: response.Company?.ToDomain(),
            contact: response.Contact?.ToDomain(),
            assignee: response.Assignee?.ToDomain()
          );
    }
}
