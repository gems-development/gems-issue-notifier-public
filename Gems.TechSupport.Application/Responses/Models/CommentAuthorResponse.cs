namespace Gems.TechSupport.Application.Responses.Models;

public record CommentAuthorResponse(long Id, string Name, string Type);

public enum CommentAuthorType
{
    Employee = 1,
    Contact
}