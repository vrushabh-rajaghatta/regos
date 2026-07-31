using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Persistence.Configurations.Organization;

public sealed class SiteIdentifierConfiguration
    : IEntityTypeConfiguration<SiteIdentifier>
{
    public void Configure(EntityTypeBuilder<SiteIdentifier> builder)
    {
        builder.ToTable("SiteIdentifiers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SiteIdentifierId(value));

        builder.Property(x => x.SchemeId)
            .HasConversion(
                id => id.Value,
                value => new IdentifierSchemeId(value))
            .IsRequired();

        // Restrict, not Cascade: a scheme in use must not be deletable out from
        // under the sites that quote it.
        builder.HasOne<IdentifierScheme>()
            .WithMany()
            .HasForeignKey(x => x.SchemeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Value)
            .HasMaxLength(SiteIdentifier.ValueMaxLength)
            .IsRequired();

        // Shadow FK to the owning site — the child holds no FK property.
        builder.Property<OrganizationSiteId>("OrganizationSiteId")
            .HasConversion(
                id => id.Value,
                value => new OrganizationSiteId(value));

        // The aggregate invariant, enforced by the database as well as by
        // AddIdentifier: a site has one FEI, and a second would mean one of
        // them is wrong rather than that the site has two. Different schemes
        // coexist freely — which is the whole reason this is a collection.
        builder.HasIndex("OrganizationSiteId", nameof(SiteIdentifier.SchemeId))
            .IsUnique();
    }
}
