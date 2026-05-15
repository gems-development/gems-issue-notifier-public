using Gems.TechSupport.Domain.Models;

namespace Gems.TechSupport.Application.Test.HelperMethods;

public static class DomainTestFactory
{
    public static Contact CreateContact(
        long id = 10,
        string fullName = "Conact FullName"
    )
    {
        return new Contact
        {
            Id = id,
            FullName = fullName
        };
    }
    public static Assignee CreateAssignee(
       long id = 20,
       string fullName = "Conact FullName"
   )
    {
        return new Assignee
        {
            Id = id,
            FullName = fullName
        };
    }
    public static Company CreateCompany(
        long id = 30,
        string companyName = "Test Company",
        Contact? contact = null
    )
    {
        return new Company
        {
            Id = id,
            CompanyName = companyName,
            Contact = contact
        };
    }
    public static Comment CreateComment(
        string content = "Content comment",
        bool isPublic = true,
        DateTime? createdAt = null,
        Contact? contact = null
    )
    {
        return new Comment
        {
            Content = content,
            Public = isPublic,
            CreatedAt = createdAt ?? new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Contact = contact ?? CreateContact()
        };
    }
}
