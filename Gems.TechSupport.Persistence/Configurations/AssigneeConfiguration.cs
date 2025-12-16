using Gems.TechSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gems.TechSupport.Persistence.Configurations;

internal sealed class AssigneeConfiguration : IEntityTypeConfiguration<Assignee>
{
    public void Configure(EntityTypeBuilder<Assignee> builder)
    {
        builder.ToTable("Assignees");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FullName).HasMaxLength(255);

        builder
            .HasMany(x => x.Issues)
            .WithOne(x => x.Assignee);
    }
}
