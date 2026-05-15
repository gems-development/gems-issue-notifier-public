using Gems.TechSupport.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gems.TechSupport.Persistence.Configurations;

internal sealed class IssueCommentAggregatorOutBoxConfiguration : IEntityTypeConfiguration<IssueCommentAggregatorOutBoxMessage>
{
    public void Configure(EntityTypeBuilder<IssueCommentAggregatorOutBoxMessage> builder)
    {
        builder.ToTable("IssueCommentAggregatorOutBox");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(255);
    }
}
