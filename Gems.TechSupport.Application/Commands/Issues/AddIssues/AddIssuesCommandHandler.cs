using AsyncKeyedLock;
using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Commands.Issues.AddComments;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Application.Commands.Issues.AddIssues;

internal sealed class AddIssueCommandHandler(IApplicationDbContext dbContext, ISender sender) 
    : ICommandHandler<AddIssuesCommand>
{
    private static readonly AsyncKeyedLocker<string> _asyncKeyedLocker = new();
    public async Task Handle(AddIssuesCommand request, CancellationToken cancellationToken)
    {
            var issues = request.Issues;
            await issues.LoadReferenceEntities(dbContext, cancellationToken);

            foreach (var issue in issues)
            {
                await HandleIssue(issue, cancellationToken);
            }
    }
    
    private async Task HandleIssue(Issue issue, CancellationToken cancellationToken)
    {
        using (await _asyncKeyedLocker.LockAsync(issue.Id.ToString(), cancellationToken))
        {
            var issueInDb = await dbContext.Issues
                .FirstOrDefaultAsync(x => x.Id == issue.Id, cancellationToken);

            if (issueInDb is not null)
            {
                return;
            }
            var addComments = new AddIssueCommentsCommand(issue);
            await sender.Send(addComments, cancellationToken);

            dbContext.Issues.Add(issue);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
