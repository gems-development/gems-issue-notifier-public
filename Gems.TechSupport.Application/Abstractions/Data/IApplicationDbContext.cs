using Gems.TechSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Issue> Issues { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Company> Companies { get; }
    DbSet<Assignee> Assignees { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
