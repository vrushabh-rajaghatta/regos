using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

namespace RegOS.Persistence.Configurations.Product;

public sealed class ProductConfiguration : IEntityTypeConfiguration<ProductAggregate>
{
    public void Configure(EntityTypeBuilder<ProductAggregate> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationId)
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder.Property(x => x.Code)
            .HasConversion(code => code.Value, value => ProductCode.Create(value))
            .HasMaxLength(ProductCode.MaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(name => name.Value, value => ProductName.Create(value))
            .HasMaxLength(ProductName.MaxLength)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(x => x.OrganizationId);

        // A product code identifies a product within its owning organization,
        // never globally. The database enforces what IProductPolicy checks, so
        // a race between two concurrent registrations still cannot duplicate.
        // No FK to Organizations: cross-context references are held by value.
        builder.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
    }
}
