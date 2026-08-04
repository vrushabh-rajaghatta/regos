using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Product;

public sealed class PackagedProductConfiguration
    : IEntityTypeConfiguration<PackagedProduct>
{
    public void Configure(EntityTypeBuilder<PackagedProduct> builder)
    {
        builder.ToTable("PackagedProducts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PackagedProductId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(
                id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(PackagedProduct.DescriptionMaxLength)
            .IsRequired();

        // Nullable as a pair, and the aggregate refuses half of one. Precision
        // matches Quantity on a component: a pack size is a count far more
        // often than a volume, but 0.5 mL is a pack size somebody sells.
        builder.Property(x => x.PackSizeQuantity)
            .HasPrecision(18, 6);

        builder.OwnsOne(x => x.PackSizeUnit, unit =>
        {
            unit.Property(x => x.System)
                .HasColumnName("PackSizeUnitSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength);

            unit.Property(x => x.Code)
                .HasColumnName("PackSizeUnitCode")
                .HasMaxLength(CodedConcept.CodeMaxLength);

            unit.Property(x => x.Display)
                .HasColumnName("PackSizeUnitDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength);
        });

        builder.Property(x => x.PackCode)
            .HasMaxLength(PackagedProduct.PackCodeMaxLength);

        // Stored, not replayed — the pack list reads one column rather than
        // reducing a history per row. Kept as int, matching MarketStatus.
        builder.Property(x => x.CurrentMarketingStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Cross-aggregate foreign key; the domain exposes no navigation
        // property. Cascade, matching the component one tier over: a pack has
        // no meaning without the market it is sold in.
        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // "What does this market sell?" — the query the aggregate exists for.
        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);

        // Deliberately NOT unique on the pack code. A market issues it, RegOS
        // does not, and a tenant migrating a portfolio may legitimately hold two
        // records mid-correction. The same call MedicinalProduct made about
        // (GlobalProductId, CountryId): uniqueness the business does not
        // guarantee is not the database's to invent.
        builder.HasIndex(x => x.PackCode);

        // Ownership: PackagedProduct (1) -> marketing status history (N). The
        // child holds no FK property, so EF uses a shadow "PackagedProductId".
        builder.HasMany(x => x.MarketingStatusHistory)
            .WithOne()
            .HasForeignKey("PackagedProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(PackagedProduct.MarketingStatusHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // "What is actually on sale in this market?" — answered off one index
        // rather than a history reduction.
        builder.HasIndex(
            x => new { x.MedicinalProductId, x.CurrentMarketingStatus });
    }
}
