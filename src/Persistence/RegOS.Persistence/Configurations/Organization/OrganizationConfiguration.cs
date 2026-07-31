using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Primitives;
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

        // The owning tenant's registry (ADR-032). Held by value, no FK to
        // Tenants — cross-context references are held by value, like
        // Products.TenantId.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

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

        builder.Property(x => x.StatusDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Acronym).HasMaxLength(50);

        builder.Property(x => x.NameNativeLanguage).HasMaxLength(250);

        // Ownership: organization (1) -> identifiers (N), shadow FK.
        builder.HasMany(x => x.Identifiers)
            .WithOne()
            .HasForeignKey("OrganizationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(OrganizationAggregate.Identifiers))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
