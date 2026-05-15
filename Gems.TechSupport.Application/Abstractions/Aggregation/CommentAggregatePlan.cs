using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Application.Abstractions.Aggregation;

public sealed record CommentAggregatePlan
    (
    long IssueId,
    long? AssigneId,
    string? ContactDisplayName,
    bool HasGreetings,
    IEnumerable<IDomainEvent> DomainEvents
);