using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Configurations.Organization;

public sealed class OrganizationConfiguration
    : IEntityTypeConfiguration<OrganizationAggregate>
{
    public void Configure(EntityTypeBuilder<OrganizationAggregate> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new OrganizationId(value));

        builder.Property(x => x.LegalName)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(x => x.LegalName);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}
