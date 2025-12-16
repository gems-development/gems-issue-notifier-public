using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Extensions;

public static class IssueExtensions
{
    public static async Task<(IReadOnlyCollection<Issue> issuesToAddInDb, IReadOnlyCollection<Issue> issuesToUpdateInDb)> SeparateForAddAndUpdate(
        this IEnumerable<Issue> issues,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var issueIds = issues.Select(x => x.Id).ToList();

        var issueIdsInDb = await dbContext.Issues
            .Where(x => issueIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var issuesToUpdateInDb = issues.Where(x => issueIdsInDb.Contains(x.Id)).ToList();
        var issuesToAddInDb = issues.Except(issuesToUpdateInDb).ToList();

        return (issuesToAddInDb, issuesToUpdateInDb);
    }

    public static async Task LoadReferenceEntities(
        this IEnumerable<Issue> issues,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var assignees = issues.Select(x => x.Assignee).OfType<Assignee>().Distinct().ToList();
        var contacts = issues.Select(x => x.Contact).OfType<Contact>().Distinct().ToList();
        var companies = issues.Select(x => x.Company).OfType<Company>().Distinct().ToList();

        var assigneeIds = assignees.Select(x => x.Id).ToList();
        var companyIds = companies.Select(x => x.Id).ToList();
        var contactIds = contacts.Select(x => x.Id).ToList();

        var assigneesInDb = await dbContext.Assignees
            .Where(x => assigneeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var contactsInDb = await dbContext.Contacts
            .Where(x => contactIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var companiesInDb = await dbContext.Companies
            .Where(x => companyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var issue in issues)
        {
            if (issue.Assignee is not null && assigneesInDb.TryGetValue(issue.Assignee.Id, out var assignee))
            {
                issue.Assignee = assignee;
            }

            if (issue.Contact is not null && contactsInDb.TryGetValue(issue.Contact.Id, out var contact))
            {
                issue.Contact = contact;
            }

            if (issue.Company is not null && companiesInDb.TryGetValue(issue.Company.Id, out var company))
            {
                issue.Company = company;
            }
        }
    }
}
