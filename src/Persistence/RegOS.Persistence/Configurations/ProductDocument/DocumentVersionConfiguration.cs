using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Persistence.Configurations.ProductDocument;

public sealed class DocumentVersionConfiguration
    : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new DocumentVersionId(value));

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.StoredFileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Checksum)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.UploadedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Shadow FK to the owning document. Declared explicitly with the
        // aggregate's strongly-typed id (and its converter) so it is
        // compatible with ProductDocument's primary key; the ownership
        // relationship binds to it in ProductDocumentConfiguration.
        builder.Property<ProductDocumentId>("ProductDocumentId")
            .HasConversion(
                id => id.Value,
                value => new ProductDocumentId(value));

        builder.HasIndex("ProductDocumentId");

        // Enforces the aggregate invariant at the database level: version
        // numbers are unique within a document.
        builder.HasIndex("ProductDocumentId", nameof(DocumentVersion.VersionNumber))
            .IsUnique();
    }
}
