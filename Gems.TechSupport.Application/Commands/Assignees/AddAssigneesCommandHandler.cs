using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Commands.Assignees;

internal sealed class AddAssigneesCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<AddAssigneesCommand>
{
    public async Task Handle(AddAssigneesCommand request, CancellationToken cancellationToken)
    {
        var assignees = request.Assignees;
        var assigneeIds = assignees
            .Select(x => x.Id)
            .ToList();

        var assigneesInDb = await dbContext.Assignees
            .AsNoTracking()
            .Where(x => assigneeIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var assigneeIdsToAddInDb = assigneeIds.Except(assigneesInDb).ToList();

        var assigneesToAddInDb = assignees.Where(x => assigneeIdsToAddInDb.Contains(x.Id)).ToList();

        dbContext.Assignees.AddRange(assigneesToAddInDb);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
