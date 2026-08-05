using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Product;

public sealed class ManufacturingOperationConfiguration
    : IEntityTypeConfiguration<ManufacturingOperation>
{
    public void Configure(EntityTypeBuilder<ManufacturingOperation> builder)
    {
        builder.ToTable("ManufacturingOperations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value, value => new ManufacturingOperationId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(
                id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.OrganizationSiteId)
            .HasConversion(
                id => id.Value, value => new OrganizationSiteId(value))
            .IsRequired();

        // Columns rather than a table: exactly one operation per row, unlike
        // the storage conditions on a shelf life, where several apply at once.
        builder.OwnsOne(x => x.Operation, operation =>
        {
            operation.Property(x => x.System)
                .HasColumnName("OperationSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            operation.Property(x => x.Code)
                .HasColumnName("OperationCode")
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            operation.Property(x => x.Display)
                .HasColumnName("OperationDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();
        });

        builder.Navigation(x => x.Operation).IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        // Null while the operation is still running. A period, not a status
        // history — the status-history rule exempts a dated fact (D5).
        builder.Property(x => x.CeasedOn);

        builder.Property(x => x.RecordedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Cascade: an operation is a statement *about* a market's product, and
        // it means nothing once that product is gone. The same call
        // PackAuthorisation makes about the pack it names.
        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict from the site, and the asymmetry is deliberate: a site is
        // registry data with its own lifecycle, and deleting one out from under
        // a recorded operation would erase where a filed product was made.
        // Sites deactivate rather than delete (ES-018), so this should never
        // fire — which is exactly when a guard is worth having.
        builder.HasOne<OrganizationSite>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        // "Which sites make this product?" — the question the aggregate exists
        // for, answered off one index.
        builder.HasIndex(x => x.MedicinalProductId);

        // The read path for "which markets does this site work for?", and the
        // covering half of the uniqueness guard below.
        builder.HasIndex(x => new { x.MedicinalProductId, x.OrganizationSiteId });

        builder.HasIndex(x => x.OrganizationSiteId);

        // NOTE — one *open* period per (market, site, operation) is enforced by
        // a filtered unique index this configuration cannot declare.
        //
        // The third column is `OperationCode`, which belongs to the owned
        // `Operation` value rather than to this entity type, and EF cannot
        // compose an index across the two — `HasIndex` resolves property names
        // against the owner alone. Both columns live in the same table, so the
        // index is valid SQL; it is simply not expressible here.
        //
        // It is therefore created by hand in the migration and is deliberately
        // absent from the model snapshot. EF only diffs what it knows about, so
        // it will never drop it — but a future scaffold will not recreate it on
        // a fresh database either, which is why this comment names the file
        // that does: AddManufacturingOperations.
        //
        // Filtered on `CeasedOn IS NULL` because the same site performing the
        // same operation over two *closed* periods is ordinary — transferred
        // away and brought back.

        builder.HasIndex(x => x.TenantId);
    }
}
