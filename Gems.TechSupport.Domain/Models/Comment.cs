using Gems.TechSupport.Domain.Primitives;

namespace Gems.TechSupport.Domain.Models;

public class Comment : Entity
{
    public required string Content { get; init; }
    public DateTime CreatedAt { get; init; }
    public Issue? Issue { get; set; }
    public required Contact Contact { get; set; }
}
