using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using ProductAggregate = RegOS.Product.Domain.Product.Product;
using CountryAggregate = RegOS.ReferenceData.Domain.Geography.Country.Country;
using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

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

        builder.Property(x => x.ProductId)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value))
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
        builder.HasOne<ProductAggregate>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
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

        // Secondary indexes on the cross-aggregate references.
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.AuthorityId);
        builder.HasIndex(x => x.ApplicantOrganizationId);

        // A product may not have two applications for the same
        // jurisdiction and authority. The database is the final line
        // of defense; the application layer enforces this too.
        builder.HasIndex(x => new
        {
            x.ProductId,
            x.CountryId,
            x.AuthorityId
        })
        .IsUnique();
    }
}
