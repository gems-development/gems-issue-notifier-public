namespace Gems.TechSupport.Domain.Primitives;

public abstract class AuditableEntity : Entity
{
    public DateTime CreationStamp { get; set; }
    public DateTime? ModificationStamp { get; set; } = null;
}
