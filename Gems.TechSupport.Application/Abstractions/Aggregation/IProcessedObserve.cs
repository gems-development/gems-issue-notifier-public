namespace Gems.TechSupport.Application.Abstractions.Aggregation;

public interface IProcessedObserver
{
    public Task<bool> ObserveMessageAsync(
        long issueId,
        int interval,
        CancellationToken cancellationToken);
}
