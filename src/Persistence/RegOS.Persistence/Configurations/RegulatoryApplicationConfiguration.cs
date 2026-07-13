using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.Persistence.Configurations;

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
                value => new ProductId(value));

        builder.Property(x => x.AuthorityId);

        builder.Property(x => x.CountryId);

        builder.Property(x => x.ApplicantOrganizationId);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ApplicationNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<int>();
    }
}
