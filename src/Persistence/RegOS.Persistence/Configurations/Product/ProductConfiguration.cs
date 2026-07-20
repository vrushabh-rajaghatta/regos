using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            .HasConversion(
                id => id.Value,
                value => new ProductId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => ProductName.Create(value))
            .HasMaxLength(ProductName.MaxLength)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}