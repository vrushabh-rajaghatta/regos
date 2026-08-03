using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Submission.Domain.Submission;

using ClinicalStudyEntity =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyEntity =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

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
                value => SubmissionDocumentId.From(value));

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

        // What this filing did to this placement, frozen at publish (ADR-045).
        // Null while a draft, and null for an attachment that was never placed —
        // an operation is a fact about a placement.
        builder.Property(x => x.Operation)
            .HasConversion<int>();

        // The placement superseded by a Replace. Held, never navigated to: it
        // names a child of a *different* Submission aggregate, and a foreign key
        // would let deleting one submission cascade into another's history.
        builder.Property(x => x.ReplacesSubmissionDocumentId)
            .HasConversion(
                id => id!.Value,
                value => SubmissionDocumentId.From(value));

        // Where the document sits in the dossier. Nullable: attached but not
        // yet placed is a legitimate state, and a submission bound to no
        // blueprint has no sections to place into at all.
        builder.Property(x => x.TemplateSectionId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new TemplateSectionId(value.Value)
                    : (TemplateSectionId?)null);

        // Which study this placement reports (ADR-056 §4). Two typed columns
        // rather than one, because they name two aggregates with two identity
        // spaces — and the exclusive-or between them is the aggregate's, since
        // no relational constraint expresses "at most one of these is set"
        // without a check constraint that would then have to be kept in step
        // with the domain by hand.
        builder.Property(x => x.ClinicalStudyId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? ClinicalStudyId.From(value.Value)
                    : null);

        builder.Property(x => x.NonClinicalStudyId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? NonClinicalStudyId.From(value.Value)
                    : null);

        // The published token, stored as written. 60 is comfortably above the
        // longest ICH publishes (inter-laboratory-standardisation-methods-
        // quality-assurance, 58) without inviting free text.
        builder.Property(x => x.FileTag).HasMaxLength(60);

        builder.Ignore(x => x.ReportsAStudy);

        // Shadow FK to the owning submission — the child holds no FK property.
        // Declared with the aggregate's strongly-typed id (and its converter)
        // so it is compatible with Submission's primary key; the ownership
        // relationship binds to it in SubmissionConfiguration.
        //
        // IsRequired is load-bearing, not decoration. SubmissionId is a
        // reference type (ES-020), so EF infers an *optional* shadow FK, and an
        // optional FK turns "remove a child" into "null the FK" rather than
        // "delete the row" — which the not-null column then rejects at
        // SaveChanges. Under the old record-struct id the non-nullability came
        // for free from the CLR type.
        builder.Property<SubmissionId>("SubmissionId")
            .HasConversion(
                id => id.Value,
                value => SubmissionId.From(value))
            .IsRequired();

        // Reference to the immutable version. No navigation (we avoid
        // cross-aggregate navigation); the FK exists only to protect the
        // referenced version from deletion while an attachment points at it.
        builder.HasOne<DocumentVersionEntity>()
            .WithMany()
            .HasForeignKey(x => x.DocumentVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // The section this document is placed in. Like the bound version on
        // Submission, a deliberate reference to a child entity of the
        // RegulatoryTemplate aggregate — placement is meaningless without
        // naming the exact immutable section. Restrict: a section a dossier is
        // organised around must never be deleted out from under it.
        builder.HasOne<TemplateSection>()
            .WithMany()
            .HasForeignKey(x => x.TemplateSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict, like the template section above and for the same reason: a
        // study a filing is organised around must not be deleted out from
        // under it. Nothing deletes a study today, which makes this the
        // constraint that keeps that true.
        builder.HasOne<ClinicalStudyEntity>()
            .WithMany()
            .HasForeignKey(x => x.ClinicalStudyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<NonClinicalStudyEntity>()
            .WithMany()
            .HasForeignKey(x => x.NonClinicalStudyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("SubmissionId");
        builder.HasIndex(x => x.DocumentVersionId);
        builder.HasIndex(x => x.TemplateSectionId);

        // S003 groups a sequence's placements by (study, eCTD element) to
        // project an STF, so these are the columns it will scan.
        builder.HasIndex(x => x.ClinicalStudyId);
        builder.HasIndex(x => x.NonClinicalStudyId);

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
