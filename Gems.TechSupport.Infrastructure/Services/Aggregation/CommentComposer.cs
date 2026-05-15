using System.Text;
using Gems.TechSupport.Application.Abstractions.Aggregation;
using Gems.TechSupport.Application.Abstractions.Okdesk;
using Gems.TechSupport.Application.Extensions;
using Gems.TechSupport.Application.Requests;
using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Events;
using Gems.TechSupport.Domain.Primitives;
using Gems.TechSupport.Infrastructure.Services.Okdesk;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz.Util;

namespace Gems.TechSupport.Infrastructure.Services.Aggregation;

internal sealed class CommentComposer
(
    IOkdeskService okdeskService,
    IOkdeskNotificationTemplatesProvider notificationProvider,
    IOptionsMonitor<OkdeskOptions> optinos,
    ILogger<CommentComposer> logger
) : ICommentComposer
{
    private readonly IOptionsMonitor<OkdeskOptions> _options = optinos;
    private static Dictionary<string, Func<IDomainEvent, IOkdeskNotificationTemplatesProvider, string?>> _eventToMessageMap = new Dictionary<string, Func<IDomainEvent, IOkdeskNotificationTemplatesProvider, string?>>
    {
        {
            nameof(IssueStatusUpdatedEvent),
            (e, templatesProvider) =>
            {
                var ev = (IssueStatusUpdatedEvent)e;
                var commentBody = HandleIssueStatusUpdatedEvent(ev,templatesProvider);
                return commentBody;
            }
        },
        {
            nameof(IssuePriorityUpdatedEvent),
            (e, templatesProvider) =>
            {
                var ev = (IssuePriorityUpdatedEvent)e;
                var commentBody = HandleIssuePriorityUpdatedEvent(ev,templatesProvider);
                return commentBody;
            }
        },
        {
            nameof(IssueDeadlineUpdatedEvent),
            (e, templatesProvider) =>
            {
                var ev = (IssueDeadlineUpdatedEvent)e;
                var commentBody = HandleIssueDeadlineUpdatedEvent(ev,templatesProvider);
                return commentBody;
            }
        },
        {
            nameof(IssueDeadlineNotificationEvent),
            (e, templatesProvider) =>
            {
                var ev = (IssueDeadlineNotificationEvent)e;
                var commentBody = HandleIssueDeadlineNotificationEvent(ev,templatesProvider);
                return commentBody;
            }
        },
        {
            nameof(IssueCompletedEvent),
            (e, templatesProvider) =>
            {
                var ev = (IssueCompletedEvent)e;
                var commentBody = templatesProvider.GetNotificationTemplate(OkdeskNotificationType.IssueCompleted);
                return commentBody;
            }
        },
        {
            nameof(IssueProblemPostCommentEvent),
            (e, templatesProvider) =>
            {
                var ev = (IssueProblemPostCommentEvent)e;
                var commentBody = templatesProvider.GetProblemTemplate(ev.ProblemName);
                return commentBody;
            }
        }
    };
    public async Task Compose(
        CommentAggregatePlan plan,
        CancellationToken ct)
    {
        var okdeskOptions = _options.CurrentValue;
        var comment = new StringBuilder();

        if (plan.HasGreetings)
        {
            var greetings = notificationProvider.GetHeaderTemplate(plan.ContactDisplayName);
            comment.AppendLine(greetings);
        }

        foreach (var ev in plan.DomainEvents)
        {
            if (!_eventToMessageMap.TryGetValue(ev.GetType().Name, out var handler))
            {
                throw new InvalidOperationException($"Событие {ev.GetType().Name} не найдено");
            }
            var text = handler(ev, notificationProvider);
            if (!string.IsNullOrWhiteSpace(text))
                comment.AppendLine(text + "<br></br>");
        }
        if (comment.ToString().IsNullOrWhiteSpace())
        {
            logger.LogWarning("Failed to aggregate comment with: {Events} events for IssueId={IssueId}", plan.DomainEvents.GetType().Name, plan.IssueId);
            return;
        }
        else
        {
            var postCommentRequest = new PostIssueCommentRequest(plan.IssueId, comment.ToString(), plan.AssigneId.Value);
            await okdeskService.PostCommentAsync(postCommentRequest, ct);
        }
    }
    private static string? HandleIssueStatusUpdatedEvent(
        IssueStatusUpdatedEvent ev,
        IOkdeskNotificationTemplatesProvider templatesProvider)
    {
        var oldTemplate = templatesProvider.GetStatusUpdatedTemplate(ev.OldStatus);
        var newTemplate = templatesProvider.GetStatusUpdatedTemplate(ev.NewStatus);

        if (oldTemplate.Equals(newTemplate))
        {
            return null;
        }

        return newTemplate;
    }
    private static string? HandleIssuePriorityUpdatedEvent(
        IssuePriorityUpdatedEvent ev,
        IOkdeskNotificationTemplatesProvider templatesProvider)
    {
        if (!string.Equals(ev.UpdateAuthorType, ConstantsForAggregation.IssueUpdateAuthorType.Employee))
        {
            return null;
        }
        if (!(ev.OldPriority != IssuePriority.Undefined &&
            (ev.NewPriority < ev.OldPriority || ev.NewPriority == IssuePriority.Highest)))
        {
            return null;
        }

        var template = ev.NewPriority switch
        {
            IssuePriority.Highest => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToHighest),
            IssuePriority.High => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToHigh),
            IssuePriority.Normal => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToNormal),
            IssuePriority.Low => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.PriorityUpdatedToLow),
            _ => string.Empty
        };
        return template;
    }
    private static string? HandleIssueDeadlineUpdatedEvent(
        IssueDeadlineUpdatedEvent ev,
        IOkdeskNotificationTemplatesProvider templatesProvider)
    {
        var template = templatesProvider.GetNotificationTemplate(OkdeskNotificationType.DeadlineUpdated);
        return template
            .Replace("[deadline_at]", ev.DeadlineAt.ToRussianStdDateTime());
    }
    private static string? HandleIssueDeadlineNotificationEvent(
        IssueDeadlineNotificationEvent ev,
        IOkdeskNotificationTemplatesProvider templatesProvider)
    {
        var template = (ev.IssueType, ev.IssuePriority) switch
        {
            (IssueType.Consultation, _) => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.Consultation),
            (IssueType.Service, _) => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.Service),
            (IssueType.Incident, IssuePriority.Highest) => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentHighest),
            (IssueType.Incident, IssuePriority.High) => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentHigh),
            (IssueType.Incident, IssuePriority.Normal) => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentNormal),
            (IssueType.Incident, IssuePriority.Low) => templatesProvider.GetNotificationTemplate(OkdeskNotificationType.IncidentLow),
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(template))
        {
            return null;
        }

        return template;
    }
}
