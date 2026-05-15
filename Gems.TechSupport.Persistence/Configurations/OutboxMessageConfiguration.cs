using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gems.TechSupport.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<DomainEventOutboxMessage>
{
    public void Configure(EntityTypeBuilder<DomainEventOutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(255);
    }
}
