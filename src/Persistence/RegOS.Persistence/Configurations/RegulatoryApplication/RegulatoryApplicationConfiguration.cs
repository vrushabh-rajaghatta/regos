using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using CountryAggregate = RegOS.ReferenceData.Domain.Geography.Country.Country;
using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;
using ApplicationTypeEntity = RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.Persistence.Configurations.RegulatoryApplication;

public sealed class RegulatoryApplicationConfiguration
    : IEntityTypeConfiguration<RegulatoryApplicationAggregate>
{
    public void Configure(
        EntityTypeBuilder<RegulatoryApplicationAggregate> builder)
    {
        builder.ToTable("RegulatoryApplications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RegulatoryApplicationId(value));

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

        builder.Property(x => x.ApplicationTypeId)
            .HasConversion(
                id => id.Value,
                value => new ApplicationTypeId(value))
            .IsRequired();

        builder.Property(x => x.ApplicantOrganizationId)
            .HasConversion(
                id => id.Value,
                value => new OrganizationId(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ApplicationNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Cross-aggregate foreign keys. The domain exposes no navigation
        // properties, but EF still models the relationships.
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
            .HasForeignKey(x => x.ApplicantOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationType is reference data. Historical applications stay
        // valid even when a type is deactivated — inactive, not deleted
        // (ES-018).
        builder.HasOne<ApplicationTypeEntity>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Secondary indexes on the cross-aggregate references.
        // The owning tenant (ADR-031). Held by value, no FK to Tenants —
        // cross-context references are held by value, like Products.TenantId.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.AuthorityId);
        builder.HasIndex(x => x.ApplicationTypeId);
        builder.HasIndex(x => x.ApplicantOrganizationId);

        // A product may not have two applications for the same
        // jurisdiction and authority. The database is the final line
        // of defense; the application layer enforces this too.
        builder.HasIndex(x => new
        {
            x.GlobalProductId,
            x.CountryId,
            x.AuthorityId
        })
        .IsUnique();

        // Ownership: RegulatoryApplication (1) -> ApplicationStudyCitations (N).
        // Cascade, because a citation has no meaning apart from the filing that
        // makes it. The shadow FK is a record-struct id, so it is non-nullable
        // for free — the trap IdentityConventionTests carries applies to
        // reference-type ids, and this aggregate still has the older shape.
        builder.HasMany(x => x.StudyCitations)
            .WithOne()
            .HasForeignKey("ApplicationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(RegulatoryApplicationAggregate.StudyCitations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
