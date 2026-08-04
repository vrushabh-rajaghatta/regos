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

        // Who may hand this pack over (ADR-061 §1's discriminator: the same
        // tablets in a 16-pack and a 100-pack are classified differently).
        // Optional as a whole — a pack is recorded before it is classified.
        builder.OwnsOne(x => x.LegalStatusOfSupply, status =>
        {
            status.Property(x => x.System)
                .HasColumnName("LegalStatusOfSupplySystem")
                .HasMaxLength(CodedConcept.SystemMaxLength);

            status.Property(x => x.Code)
                .HasColumnName("LegalStatusOfSupplyCode")
                .HasMaxLength(CodedConcept.CodeMaxLength);

            status.Property(x => x.Display)
                .HasColumnName("LegalStatusOfSupplyDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength);
        });

        // The shelf-life statement, mapped as ONE owned value object because the
        // period is only true under the conditions.
        //
        // Required, and that is the load-bearing word. EF reads an *optional*
        // owned reference back as null when every column it shares is null —
        // and a pack whose only statement is "protect from light" has exactly
        // that, so its conditions would vanish on reload while the write
        // succeeded. A required dependent is materialised unconditionally, and
        // ShelfLifeStorage.NotStated is the empty value that makes it honest.
        builder.OwnsOne(x => x.ShelfLife, shelf =>
        {
            shelf.Property(x => x.Value)
                .HasColumnName("ShelfLifeValue")
                .HasPrecision(18, 6);

            shelf.Property(x => x.Text)
                .HasColumnName("ShelfLifeText")
                .HasMaxLength(ShelfLifeStorage.TextMaxLength);

            // Kept literal — 3 YEAR is stored as 3 YEAR, never normalised to
            // 36 MONTH. Its own vocabulary, not MeasurementVocabulary, so that
            // "500 months" can never be a strength.
            shelf.OwnsOne(x => x.Unit, unit =>
            {
                unit.Property(x => x.System)
                    .HasColumnName("ShelfLifeUnitSystem")
                    .HasMaxLength(CodedConcept.SystemMaxLength);

                unit.Property(x => x.Code)
                    .HasColumnName("ShelfLifeUnitCode")
                    .HasMaxLength(CodedConcept.CodeMaxLength);

                unit.Property(x => x.Display)
                    .HasColumnName("ShelfLifeUnitDisplay")
                    .HasMaxLength(CodedConcept.DisplayMaxLength);
            });

            // A table rather than columns, because several precautions
            // ordinarily apply at once — the same call RoutesOfAdministration
            // made one aggregate over.
            shelf.OwnsMany(x => x.StorageConditions, condition =>
            {
                condition.ToTable("PackagedProductStorageConditions");

                condition.WithOwner().HasForeignKey("PackagedProductId");

                condition.Property(x => x.System)
                    .HasMaxLength(CodedConcept.SystemMaxLength)
                    .IsRequired();

                condition.Property(x => x.Code)
                    .HasMaxLength(CodedConcept.CodeMaxLength)
                    .IsRequired();

                condition.Property(x => x.Display)
                    .HasMaxLength(CodedConcept.DisplayMaxLength)
                    .IsRequired();

                // One statement of each condition per pack, enforced where a
                // race cannot slip past the value object's own check.
                condition.HasIndex("PackagedProductId", "Code").IsUnique();
            });
        });

        builder.Navigation(x => x.ShelfLife).IsRequired();

        builder.Metadata
            .FindNavigation(nameof(PackagedProduct.ShelfLife))!
            .TargetEntityType
            .FindNavigation(nameof(ShelfLifeStorage.StorageConditions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

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
