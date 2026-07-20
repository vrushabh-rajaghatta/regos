using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Persistence.ReadModels;

namespace RegOS.Persistence.Configurations.Product;

/// <summary>
/// Maps the read-only product directory projection onto the existing Products
/// table. ToView keeps it read-only and out of migrations: it owns no schema,
/// it only reads the table the Product aggregate owns.
/// </summary>
public sealed class ProductDirectoryRowConfiguration
    : IEntityTypeConfiguration<ProductDirectoryRow>
{
    public void Configure(EntityTypeBuilder<ProductDirectoryRow> builder)
    {
        builder.HasNoKey();

        builder.ToView("Products");

        // Type and Status are persisted as strings by ProductConfiguration, so
        // the row must convert them the same way or the read would fail.
        builder.Property(x => x.Type).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();
    }
}
