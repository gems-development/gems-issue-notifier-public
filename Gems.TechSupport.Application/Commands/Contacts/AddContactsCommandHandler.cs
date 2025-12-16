using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Commands.Contacts;

internal sealed class AddContactsCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<AddContactsCommand>
{
    public async Task Handle(AddContactsCommand request, CancellationToken cancellationToken)
    {
        var contacts = request.Contacts;
        var contactIds = contacts
            .Select(x => x.Id)
            .ToList();

        var contactIdsInDb = await dbContext.Contacts
            .AsNoTracking()
            .Where(x => contactIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var contactIdsToAddInDb = contactIds.Except(contactIdsInDb).ToList();

        var contactsToAddInDb = contacts.Where(x => x is not null && contactIdsToAddInDb.Contains(x.Id)).ToList();

        dbContext.Contacts.AddRange(contactsToAddInDb);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
