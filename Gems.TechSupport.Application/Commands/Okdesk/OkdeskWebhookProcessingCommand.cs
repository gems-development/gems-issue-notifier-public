using Gems.TechSupport.Application.Responses.Webhooks;
using Gems.TechSupport.Domain.Models;
using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Okdesk;

public record OkdeskWebhookProcessingCommand(Issue Issue, WebhookEvent WebhookEvent) : ICommand;
