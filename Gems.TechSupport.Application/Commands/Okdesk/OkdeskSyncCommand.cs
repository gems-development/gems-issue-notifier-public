using Gems.TechSupport.Domain.Shared.CQRS;

namespace Gems.TechSupport.Application.Commands.Okdesk;

public record OkdeskSyncCommand(DateTime UpdatedSince, DateTime UpdatedUntil, int? PageSize = null) : ICommand;
