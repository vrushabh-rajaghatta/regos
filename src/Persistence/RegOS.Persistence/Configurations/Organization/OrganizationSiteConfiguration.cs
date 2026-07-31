using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Primitives;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Configurations.Organization;

public sealed class OrganizationSiteConfiguration
    : IEntityTypeConfiguration<OrganizationSite>
{
    public void Configure(EntityTypeBuilder<OrganizationSite> builder)
    {
        builder.ToTable("OrganizationSites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new OrganizationSiteId(value));

        // Its own tenant, and its own fail-closed filter in RegOSDbContext: a
        // site is a root reachable through the directory, not only through its
        // organization, so it cannot inherit the parent's protection.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.Property(x => x.OrganizationId)
            .HasConversion(
                id => id.Value,
                value => new OrganizationId(value))
            .IsRequired();

        builder.HasOne<OrganizationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizationId);

        builder.Property(x => x.Name)
            .HasMaxLength(OrganizationSite.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name);

        builder.Property(x => x.NameNativeLanguage)
            .HasMaxLength(OrganizationSite.NameMaxLength);

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.StatusDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Phone).HasMaxLength(50);

        // The address stays inline in this table rather than leaking seven
        // descriptive columns onto the aggregate's surface.
        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.CountryId)
                .HasColumnName("CountryId")
                .HasConversion(
                    id => id.Value,
                    value => new CountryId(value))
                .IsRequired();

            address.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            address.Property(x => x.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(PostalAddress.LineMaxLength);

            address.Property(x => x.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(PostalAddress.LineMaxLength);

            address.Property(x => x.Line3)
                .HasColumnName("AddressLine3")
                .HasMaxLength(PostalAddress.LineMaxLength);

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(PostalAddress.LineMaxLength);

            address.Property(x => x.StateProvince)
                .HasColumnName("StateProvince")
                .HasMaxLength(PostalAddress.LineMaxLength);

            address.Property(x => x.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(30);

            // The directory's whole question is "which sites are in India?",
            // and its second filter is the type — so the index carries both.
            address.HasIndex(x => x.CountryId);
        });

        builder.Navigation(x => x.Address).IsRequired();

        // "Which manufacturing sites do we have in India?" — the query that
        // justifies this being a root, so it is indexed rather than scanned.
        builder.HasIndex(x => new { x.TenantId, x.Type });

        // Ownership: site (1) -> identifiers (N). The child holds no FK
        // property, so EF uses a shadow "OrganizationSiteId".
        builder.HasMany(x => x.Identifiers)
            .WithOne()
            .HasForeignKey("OrganizationSiteId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(OrganizationSite.Identifiers))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
