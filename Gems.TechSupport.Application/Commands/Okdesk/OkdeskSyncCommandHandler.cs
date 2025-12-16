using Gems.TechSupport.Application.Abstractions.Data;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Commands.Assignees;
using Gems.TechSupport.Application.Commands.Companies;
using Gems.TechSupport.Application.Commands.Contacts;
using Gems.TechSupport.Application.Commands.Issues.AddIssues;
using Gems.TechSupport.Application.Commands.Issues.UpdateIssues;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;
using MediatR;
using Microsoft.FeatureManagement;

namespace Gems.TechSupport.Application.Commands.Okdesk;

internal sealed class OkdeskSyncCommandHandler(
    IOkdeskService okdeskService,
    IApplicationDbContext dbContext,
    ISender sender,
    IFeatureManager featureManager) 
    : ICommandHandler<OkdeskSyncCommand>
{
    public async Task Handle(OkdeskSyncCommand request, CancellationToken cancellationToken)
    {
        var getUpdatedIssuesRequest = new GetUpdatedIssuesRequest(request.UpdatedSince, request.UpdatedUntil, request.PageSize);

        var issueList = okdeskService.GetUpdatedIssuesAsync(getUpdatedIssuesRequest, cancellationToken);

        await foreach (var issues in issueList)
        {
            await SyncReferenceEntities(issues, cancellationToken);

            var skitIssues = issues
                .Where(x => x.IsSkitType)
                .ToList();

            if (await IsSkitIssuesProcessingEnabled())
            {
                var skitCommand = new OkdeskSkitIssuesCommand(skitIssues, request.PageSize);
                await sender.Send(skitCommand, cancellationToken);
            }

            var issuesWithoutSkit = issues.Except(skitIssues).ToList();

            var (issuesToAddInDb, issuesToUpdateInDb) = await issues.SeparateForAddAndUpdate(dbContext, cancellationToken);

            var addIssuesCommand = new AddIssuesCommand(issuesToAddInDb);
            var updateIssuesCommand = new UpdateIssuesCommand(issuesToUpdateInDb);

            await sender.Send(addIssuesCommand, cancellationToken);
            await sender.Send(updateIssuesCommand, cancellationToken);
        }
    }

    private async Task SyncReferenceEntities(IReadOnlyCollection<Issue> issues, CancellationToken cancellationToken)
    {
        var assignees = issues.Select(x => x.Assignee).OfType<Assignee>().Distinct().ToList();
        var companies = issues.Select(x => x.Company).OfType<Company>().Distinct().ToList();
        var contacts = issues.Select(x => x.Contact).OfType<Contact>().Distinct().ToList();

        var addAssigneesCommand = new AddAssigneesCommand(assignees);
        var addCompaniesCommand = new AddCompaniesCommand(companies);
        var addContactsCommand = new AddContactsCommand(contacts);

        await sender.Send(addAssigneesCommand, cancellationToken);
        await sender.Send(addCompaniesCommand, cancellationToken);
        await sender.Send(addContactsCommand, cancellationToken);
    }

    private Task<bool> IsSkitIssuesProcessingEnabled()
    {
        return featureManager.IsEnabledAsync(Constants.OkdeskFeatures.SkitIssuesProcessing);
    }
}
