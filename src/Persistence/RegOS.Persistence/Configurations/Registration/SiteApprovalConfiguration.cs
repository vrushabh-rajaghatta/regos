using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.Registration.Domain.Aggregates.SiteApprovals;
using RegOS.SharedKernel.Primitives;

using RegistrationAggregate =
    RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Persistence.Configurations.Registration;

public sealed class SiteApprovalConfiguration
    : IEntityTypeConfiguration<SiteApproval>
{
    public void Configure(EntityTypeBuilder<SiteApproval> builder)
    {
        builder.ToTable("SiteApprovals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SiteApprovalId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.RegistrationId)
            .HasConversion(id => id.Value, value => new RegistrationId(value))
            .IsRequired();

        builder.Property(x => x.OrganizationSiteId)
            .HasConversion(
                id => id.Value, value => new OrganizationSiteId(value))
            .IsRequired();

        builder.Property(x => x.ApprovedOn)
            .IsRequired();

        builder.Property(x => x.RecordedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Cascade from the licence, because an approval is a statement made
        // *by* it and means nothing once it is gone — the same call
        // PackAuthorisation makes.
        builder.HasOne<RegistrationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict from the site, and the asymmetry matches ManufacturingOperation
        // one context over: deleting a plant out from under an approval would
        // erase which sites a filed licence named. Sites deactivate rather than
        // delete (ES-018), so this should never fire.
        builder.HasOne<OrganizationSite>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        // One statement per (licence, site), enforced where a race cannot slip
        // past the handler's own check. Deliberately *not* unique on the site
        // alone: a site approved on two licences is ordinary — one plant
        // supplies several markets, which is the case the divergence exists to
        // reason about.
        builder.HasIndex(x => new { x.RegistrationId, x.OrganizationSiteId })
            .IsUnique();

        // "Which sites does this market's licences approve?" walks in from the
        // site side, so this is the index the divergence read goes through.
        builder.HasIndex(x => x.OrganizationSiteId);

        builder.HasIndex(x => x.TenantId);
    }
}
