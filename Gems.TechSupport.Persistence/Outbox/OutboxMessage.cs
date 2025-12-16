namespace Gems.TechSupport.Persistence.Outbox;

public class OutboxMessage
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Content { get; init; }
    public DateTime OccuredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
