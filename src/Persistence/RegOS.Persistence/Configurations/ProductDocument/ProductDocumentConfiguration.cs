using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.SharedKernel.Primitives;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;

using ProductDocumentEntity = RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using ProductAggregate = RegOS.Product.Domain.Product.Product;
using DocumentTypeEntity = RegOS.ReferenceData.Domain.DocumentType.DocumentType;

namespace RegOS.Persistence.Configurations.ProductDocument;

public sealed class ProductDocumentConfiguration
    : IEntityTypeConfiguration<ProductDocumentEntity>
{
    public void Configure(EntityTypeBuilder<ProductDocumentEntity> builder)
    {
        builder.ToTable("ProductDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ProductDocumentId(value));

        // The owning tenant (ADR-031), derived from the owning product at
        // upload. Held by value, no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.Property(x => x.ProductId)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value))
            .IsRequired();

        builder.Property(x => x.DocumentTypeId)
            .HasConversion(
                id => id.Value,
                value => new DocumentTypeId(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(ProductDocumentEntity.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // Current-version pointer stored as a plain (converted) column — no
        // FK. Modelling it as a foreign key creates an insert/delete cycle
        // with the ownership relationship below; the aggregate already
        // guarantees the pointer only ever references one of its own
        // versions, so the invariant is enforced in the domain.
        builder.Property(x => x.CurrentVersionId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new DocumentVersionId(value.Value)
                    : (DocumentVersionId?)null);

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Product (N:1) — a product owns its documents; deleting the product
        // removes them.
        builder.HasOne<ProductAggregate>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // DocumentType (N:1) — reference data is protected from deletion.
        builder.HasOne<DocumentTypeEntity>()
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ownership: ProductDocument (1) -> DocumentVersions (N). The child
        // has no FK property, so EF uses a shadow "ProductDocumentId".
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("ProductDocumentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(ProductDocumentEntity.Versions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.DocumentTypeId);
        builder.HasIndex(x => x.Status);

        // Within a single product, document names are unique.
        builder.HasIndex(x => new { x.ProductId, x.Name })
            .IsUnique();
    }
}
