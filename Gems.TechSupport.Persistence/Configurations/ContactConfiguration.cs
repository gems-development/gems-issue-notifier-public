using Gems.TechSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gems.TechSupport.Persistence.Configurations;

internal sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder
            .HasMany(x => x.Issues)
            .WithOne(x => x.Contact);

        builder
            .HasMany(x => x.Comments)
            .WithOne(x => x.Contact);

        builder
            .HasMany(x => x.Companies)
            .WithOne(x => x.Contact);
    }
}
