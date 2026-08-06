using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;
using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Persistence.Configurations.Registration;

public sealed class RegistrationConfiguration
    : IEntityTypeConfiguration<RegistrationAggregate>
{
    public void Configure(EntityTypeBuilder<RegistrationAggregate> builder)
    {
        builder.ToTable("Registrations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RegistrationId(value));

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(
                id => id.Value,
                value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(
                id => id.Value,
                value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.HolderOrganizationId)
            .HasConversion(
                id => id.Value,
                value => new OrganizationId(value))
            .IsRequired();

        // Null for acquired, in-licensed or migrated authorisations, whose
        // filing RegOS never witnessed.
        builder.Property(x => x.OriginatingApplicationId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new RegulatoryApplicationId(value.Value)
                    : (RegulatoryApplicationId?)null);

        builder.Property(x => x.RegistrationNumber)
            .HasMaxLength(RegistrationAggregate.RegistrationNumberMaxLength);

        builder.Property(x => x.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        // Calendar facts in a jurisdiction, not instants.
        builder.Property(x => x.ApprovedOn).HasColumnType("date");
        builder.Property(x => x.ExpiresOn).HasColumnType("date");

        builder.Property(x => x.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Cross-aggregate foreign keys. The domain exposes no navigation
        // properties, but EF still models the relationships. All Restrict: a
        // market presence, authority or holder referenced by an authorisation
        // must never be deleted out from under it.
        //
        // There is no FK to Products or Countries any more. Both are reached
        // through the medicinal product, which is the whole point — a database
        // that cannot store a registration disagreeing with its own market.
        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<OrganizationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.HolderOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RegulatoryApplicationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.OriginatingApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.OriginatingApplicationId);

        // Both portfolio questions — "where is this product registered?" and
        // "what do we hold in this market?" — now enter through the medicinal
        // product, so this is the index they share. The status is kept on it
        // because the market view leads with live authorisations.
        builder.HasIndex(x => new { x.MedicinalProductId, x.CurrentStatus });

        // Deliberately NOT unique on MedicinalProductId: a market-local product
        // legitimately holds several authorisations — different strengths,
        // presentations, or holders after a partial divestment. The
        // registration number is the business identity, not the market.

        // Ownership: Registration (1) -> status history (N). The child holds no
        // FK property, so EF uses a shadow "RegistrationId".
        builder.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey("RegistrationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(RegistrationAggregate.History))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // ADR-065 D2 — an annotation the owning aggregate holds. SetNull, not
        // Cascade: deleting a plan step must never delete a registration, and I9 makes
        // the resulting null mean exactly what every other null here means —
        // nothing.
        builder.Property(x => x.ProcessStepId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new ProcessStepId(value.Value) : null);

        builder.HasOne<ProcessStep>()
            .WithMany()
            .HasForeignKey(x => x.ProcessStepId)
            .OnDelete(DeleteBehavior.SetNull);

        // "What did this step produce?" — the read the link exists for.
        builder.HasIndex(x => x.ProcessStepId);
    }
}
