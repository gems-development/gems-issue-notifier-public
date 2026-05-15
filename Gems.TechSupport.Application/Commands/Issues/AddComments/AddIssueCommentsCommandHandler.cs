using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Commands.Contacts;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gems.TechSupport.Application.Commands.Issues.AddComments;

internal sealed class AddIssueCommentsCommandHandler(
        IApplicationDbContext dbContext,
        IOkdeskService okdeskService,
        ISender sender,
        ILogger<AddIssueCommentsCommandHandler> logger)
    : ICommandHandler<AddIssueCommentsCommand>
{
    public async Task Handle(AddIssueCommentsCommand request, CancellationToken cancellationToken)
    {
        var issue = request.Issue;

        if (issue.Contact is null)
        {
            try
            {
                var detailedIssue = await okdeskService.GetIssueDetailsByIdAsync(
                    new GetIssueDetailsByIdRequest(issue.Id),
                    cancellationToken);
                issue.Contact = detailedIssue.Contact;
                issue.Company = detailedIssue.Company;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при загрузке деталей для заявки {Id}", issue.Id);
            }
        }

        var query = new GetIssueCommentsRequest(issue.Id);
        var comments = await okdeskService.GetIssueCommentsAsync(query, cancellationToken);

        var contactsFromComments = comments
            .Select(c => c.Contact)
            .OfType<Contact>()
            .Distinct()
            .ToList();

        if (contactsFromComments.Count > 0)
        {
            await EnsureContactsExistAsync(contactsFromComments, cancellationToken);
        }

        var contactIdsFromComments = contactsFromComments.Select(c => c.Id).ToList();
        var contactsInDb = await dbContext.Contacts
            .Where(c => contactIdsFromComments.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        foreach (var comment in comments)
        {
            if (comment.Contact is null)
                continue;

            if (contactsInDb.TryGetValue(comment.Contact.Id, out var contact))
            {
                comment.Contact = contact;
                issue.AddComment(comment);
            }
        }
    }

    private async Task EnsureContactsExistAsync(List<Contact> contactsFromComments, CancellationToken cancellationToken)
    {
        var contactId = contactsFromComments.Select(c => c.Id).ToList();

        var existingContactId = await dbContext.Contacts
            .Where(c => contactId.Contains((int)c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var contactToAdd = contactsFromComments
            .Where(c => !existingContactId.Contains(c.Id))
            .ToList();

        if (contactId.Count > 0)
        {
            var addContactsCommand = new AddContactsCommand(contactToAdd);
            await sender.Send(addContactsCommand, cancellationToken);
        }
    }
}
