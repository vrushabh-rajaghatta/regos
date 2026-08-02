using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SubmissionDeletionConfiguration
    : IEntityTypeConfiguration<SubmissionDeletion>
{
    public void Configure(EntityTypeBuilder<SubmissionDeletion> builder)
    {
        builder.ToTable("SubmissionDeletions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SubmissionDeletionId.From(value));

        builder.Property(x => x.ProductDocumentId)
            .HasConversion(id => id.Value, value => new ProductDocumentId(value))
            .IsRequired();

        builder.Property(x => x.TemplateSectionId)
            .HasConversion(id => id.Value, value => new TemplateSectionId(value))
            .IsRequired();

        // Held, never navigated to: it names a child of a *different* Submission
        // aggregate, and a foreign key would let deleting one submission cascade
        // into another filing's record of what it withdrew.
        builder.Property(x => x.DeletesSubmissionDocumentId)
            .HasConversion(id => id.Value, value => SubmissionDocumentId.From(value))
            .IsRequired();

        // Shadow FK to the owning submission. IsRequired because a reference-type
        // id makes the inferred FK optional, and an optional FK severs instead of
        // deleting (ADR-043 migration note).
        builder.Property<SubmissionId>("SubmissionId")
            .HasConversion(id => id.Value, value => SubmissionId.From(value))
            .IsRequired();

        builder.HasIndex("SubmissionId");

        // A filing withdraws a given placement once. Mirrors the aggregate: the
        // deletions are computed from a set, so a duplicate would be corruption.
        builder.HasIndex(
                "SubmissionId",
                nameof(SubmissionDeletion.ProductDocumentId),
                nameof(SubmissionDeletion.TemplateSectionId))
            .IsUnique();
    }
}
