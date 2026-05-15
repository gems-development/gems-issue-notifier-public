using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Options;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Shared.CQRS;
using Microsoft.Extensions.Options;

namespace Gems.TechSupport.Application.EventHandlers;

internal sealed class IssueProblemAutoCompletedEventHandler(
    IOptionsMonitor<ProblemTemplatesOptions> autoCompletedOptions,
    IOkdeskService okdeskService)
    : IDomainEventHandler<IssueAutoCompletedEvent>
{

    public async Task Handle(IssueAutoCompletedEvent notification, CancellationToken cancellationToken)
    {
        var templates = autoCompletedOptions.CurrentValue;
        var template = templates.Templates
            .FirstOrDefault(x => string.Equals(
                x.Key,
                notification.AutoValue,
                StringComparison.OrdinalIgnoreCase))
            .SolutionParameters;

        if (string.IsNullOrWhiteSpace(template))
        {
            throw new InvalidOperationException($"Auto-complete template not found for '{notification.AutoValue}'.");
        }

        var statusRequest = new SetIssueAutoCompletedStatusRequest(
            notification.IssueId,
            IssueStatus.Completed,
            templates.UnPublicComment,
            template,
            templates.TimeEntry
        );
        await okdeskService.SetIssueAutoCompletedStatusAsync(statusRequest, cancellationToken);
    }
}
