using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Primitives;


namespace RegOS.Persistence.Configurations.Product;

public sealed class ProductConfiguration : IEntityTypeConfiguration<GlobalProduct>
{
    public void Configure(EntityTypeBuilder<GlobalProduct> builder)
    {
        // Still "Products", not "GlobalProducts". EPIC-017 S000 renamed the
        // aggregate and its id; the table keeps its name because renaming it
        // buys nothing a reader needs — the tier is unambiguous from the type —
        // and would cost a second rename when the sibling MedicinalProducts
        // table arrives. Stated here so it reads as a decision rather than a
        // miss. The ProductId columns *did* follow, because a column that
        // disagrees with its property is a silent trap.
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new GlobalProductId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
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

        builder.HasIndex(x => x.TenantId);

        // A product code identifies a product within its owning tenant,
        // never globally. The database enforces what IProductPolicy checks, so
        // a race between two concurrent registrations still cannot duplicate.
        // No FK to Tenants: cross-context references are held by value.
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
