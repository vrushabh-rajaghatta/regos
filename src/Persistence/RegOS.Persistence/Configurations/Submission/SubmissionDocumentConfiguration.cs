using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Submission;

using DocumentVersionEntity = RegOS.ProductDocument.Domain.Entities.DocumentVersion;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SubmissionDocumentConfiguration
    : IEntityTypeConfiguration<SubmissionDocument>
{
    public void Configure(EntityTypeBuilder<SubmissionDocument> builder)
    {
        builder.ToTable("SubmissionDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SubmissionDocumentId(value));

        builder.Property(x => x.ProductDocumentId)
            .HasConversion(
                id => id.Value,
                value => new ProductDocumentId(value))
            .IsRequired();

        builder.Property(x => x.DocumentVersionId)
            .HasConversion(
                id => id.Value,
                value => new DocumentVersionId(value))
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.AttachedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Shadow FK to the owning submission — the child holds no FK property.
        // Declared with the aggregate's strongly-typed id (and its converter)
        // so it is compatible with Submission's primary key; the ownership
        // relationship binds to it in SubmissionConfiguration.
        builder.Property<SubmissionId>("SubmissionId")
            .HasConversion(
                id => id.Value,
                value => new SubmissionId(value));

        // Reference to the immutable version. No navigation (we avoid
        // cross-aggregate navigation); the FK exists only to protect the
        // referenced version from deletion while an attachment points at it.
        builder.HasOne<DocumentVersionEntity>()
            .WithMany()
            .HasForeignKey(x => x.DocumentVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("SubmissionId");
        builder.HasIndex(x => x.DocumentVersionId);

        // Mirrors the aggregate invariant: a Product Document may appear only
        // once per submission. Guards against corruption from concurrent
        // updates or manual data changes.
        builder.HasIndex(
                "SubmissionId",
                nameof(SubmissionDocument.ProductDocumentId))
            .IsUnique();

        // Display order is unique within a submission.
        builder.HasIndex(
                "SubmissionId",
                nameof(SubmissionDocument.DisplayOrder))
            .IsUnique();
    }
}
