using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;
using CountryAggregate = RegOS.ReferenceData.Domain.Geography.Country.Country;
using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

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

        builder.Property(x => x.GlobalProductId)
            .HasConversion(
                id => id.Value,
                value => new GlobalProductId(value))
            .IsRequired();

        builder.Property(x => x.CountryId)
            .HasConversion(
                id => id.Value,
                value => new CountryId(value))
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
        // product, market or holder referenced by an authorisation must never
        // be deleted out from under it.
        builder.HasOne<GlobalProduct>()
            .WithMany()
            .HasForeignKey(x => x.GlobalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CountryAggregate>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
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
        builder.HasIndex(x => x.GlobalProductId);
        builder.HasIndex(x => x.OriginatingApplicationId);

        // The portfolio questions: "where is this product registered?" and
        // "what do we hold in this market?" (STORY-003).
        builder.HasIndex(x => new { x.CountryId, x.CurrentStatus });

        // Deliberately NOT unique on (GlobalProductId, CountryId): a product
        // legitimately holds several authorisations in one market — different
        // strengths, presentations, or holders after a partial divestment. The
        // registration number is the business identity, not the jurisdiction.

        // Ownership: Registration (1) -> status history (N). The child holds no
        // FK property, so EF uses a shadow "RegistrationId".
        builder.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey("RegistrationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(RegistrationAggregate.History))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
