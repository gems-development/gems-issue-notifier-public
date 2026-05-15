using Gems.TechSupport.Application.Abstractions.Aggregation;
using Gems.TechSupport.Persistence;
using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Gems.TechSupport.Infrastructure.Services.Aggregation;

internal sealed class ProcessedObserver(ApplicationDbContext dbContext) : IProcessedObserver
{
    public async Task<bool> ObserveMessageAsync(
         long issueId,
         int interval,
         CancellationToken cancellationToken)
    {

        var firstUnprocessedEvent = await dbContext
            .Set<IssueCommentAggregatorOutBoxMessage>()
            .AsNoTracking()
            .Where(x => x.IssueId == issueId && x.ProcessedOnUtc == null)
            .MinAsync(x => (DateTime?)x.OccuredOnUtc, cancellationToken);

        if (firstUnprocessedEvent is null)
        {
            return false;
        }

        var lastProcessedEvent = await dbContext
            .Set<IssueCommentAggregatorOutBoxMessage>()
            .AsNoTracking()
            .Where(x => x.IssueId == issueId && x.ProcessedOnUtc != null)
            .MaxAsync(x => (DateTime?)x.ProcessedOnUtc, cancellationToken);

        if (lastProcessedEvent is null)
        {
            return true;
        }

        var alreadyProcessedInWindow = firstUnprocessedEvent.Value - lastProcessedEvent.Value;

        return alreadyProcessedInWindow > TimeSpan.FromHours(interval);
    }
}
