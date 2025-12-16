using Gems.TechSupport.Domain.Enums;
using Gems.TechSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gems.TechSupport.Persistence.Configurations;

internal sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Status)
            .HasMaxLength(20)
            .HasConversion(
                x => x == null ? null : x.ToString(),
                status => string.IsNullOrEmpty(status) ? null : Enum.Parse<IssueStatus>(status)
            );

        builder.Property(x => x.Priority)
            .HasMaxLength(20)
            .HasConversion(
                x => x == null ? null : x.ToString(),
                priority => string.IsNullOrEmpty(priority) ? null : Enum.Parse<IssuePriority>(priority)
            );

        builder.Property(x => x.Type)
            .HasMaxLength(20)
            .HasConversion(
                x => x == null ? null : x.ToString(),
                type => string.IsNullOrEmpty(type) ? null : Enum.Parse<IssueType>(type)
            );
    }
}
