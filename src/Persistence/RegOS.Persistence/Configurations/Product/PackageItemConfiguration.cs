using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Product;

public sealed class PackageItemConfiguration
    : IEntityTypeConfiguration<PackageItem>
{
    public void Configure(EntityTypeBuilder<PackageItem> builder)
    {
        builder.ToTable("PackageItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PackageItemId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.PackagedProductId)
            .HasConversion(id => id.Value, value => new PackagedProductId(value))
            .IsRequired();

        // Nullable on purpose: null is the outermost layer, not a missing one.
        builder.Property(x => x.ParentPackageItemId)
            .HasConversion(
                id => id!.Value, value => new PackageItemId(value));

        builder.OwnsOne(x => x.ItemType, c => Columns(c, "ItemType"));
        builder.Navigation(x => x.ItemType).IsRequired();

        builder.OwnsOne(x => x.Material, c => Columns(c, "Material"));

        builder.OwnsOne(
            x => x.UnitOfPresentation, c => Columns(c, "UnitOfPresentation"));

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(PackageItem.DescriptionMaxLength);

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Self-referencing, and deliberately Restrict rather than Cascade: the
        // domain refuses to remove a layer that still holds others, so a
        // cascade here would be a second, silent answer to a question the
        // aggregate already answers out loud. The same call
        // MedicinalProductComponent made.
        //
        // Nothing in the schema says the graph is acyclic — Postgres cannot
        // express that for an adjacency list without a trigger, and the rule
        // lives in PackagingTree instead.
        builder.HasOne<PackageItem>()
            .WithMany()
            .HasForeignKey(x => x.ParentPackageItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PackagedProduct>()
            .WithMany()
            .HasForeignKey(x => x.PackagedProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Every rule about shape loads by pack, so this is the index that every
        // write as well as every read goes through.
        builder.HasIndex(x => x.PackagedProductId);

        builder.HasIndex(x => x.TenantId);
    }

    /// <remarks>
    /// The column names are the only thing that differs between the three, so
    /// they are the only thing parameterised — the same shape
    /// <c>MedicinalProductComponentConfiguration</c> uses, and deliberately not
    /// shared with it: an owned navigation builder is typed to its owner, and a
    /// helper spanning both would need a generic that says nothing.
    /// </remarks>
    private static void Columns(
        OwnedNavigationBuilder<PackageItem, CodedConcept> owned,
        string prefix)
    {
        owned.Property(x => x.System)
            .HasColumnName($"{prefix}System")
            .HasMaxLength(CodedConcept.SystemMaxLength);

        owned.Property(x => x.Code)
            .HasColumnName($"{prefix}Code")
            .HasMaxLength(CodedConcept.CodeMaxLength);

        owned.Property(x => x.Display)
            .HasColumnName($"{prefix}Display")
            .HasMaxLength(CodedConcept.DisplayMaxLength);
    }
}
