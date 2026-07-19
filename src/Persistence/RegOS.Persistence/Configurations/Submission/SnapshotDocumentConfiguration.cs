using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Snapshot;

using DocumentVersionEntity = RegOS.ProductDocument.Domain.Entities.DocumentVersion;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SnapshotDocumentConfiguration
    : IEntityTypeConfiguration<SnapshotDocument>
{
    public void Configure(EntityTypeBuilder<SnapshotDocument> builder)
    {
        builder.ToTable("SubmissionSnapshotDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SnapshotDocumentId(value));

        builder.Property(x => x.DocumentVersionId)
            .HasConversion(
                id => id.Value,
                value => new DocumentVersionId(value))
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        // Shadow FK to the owning snapshot — the child holds no FK property.
        // Declared with the aggregate's strongly-typed id (and its converter) so
        // it is compatible with SubmissionSnapshot's primary key.
        builder.Property<SubmissionSnapshotId>("SubmissionSnapshotId")
            .HasConversion(
                id => id.Value,
                value => new SubmissionSnapshotId(value));

        // Reference to the immutable version. No navigation; the FK exists only to
        // protect the referenced version from deletion while a published dossier
        // points at it — the guarantee that history never drifts.
        builder.HasOne<DocumentVersionEntity>()
            .WithMany()
            .HasForeignKey(x => x.DocumentVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("SubmissionSnapshotId");
        builder.HasIndex(x => x.DocumentVersionId);

        // Display order is unique within a snapshot (Invariant 3), enforced in
        // the database as well as the aggregate.
        builder.HasIndex(
                "SubmissionSnapshotId",
                nameof(SnapshotDocument.DisplayOrder))
            .IsUnique();
    }
}
