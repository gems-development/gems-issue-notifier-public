using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Persistence.Configurations;
using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Persistence;

public sealed class ApplicationDbContext(DbContextOptions options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Issue> Issues { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Assignee> Assignees { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<DomainEventOutboxMessage> OutboxMessages { get; set; }
    public DbSet<IssueCommentAggregatorOutBoxMessage> IssueCommentAggregatorOutBox { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new IssueCommentAggregatorOutBoxConfiguration());
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
