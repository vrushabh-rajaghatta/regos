using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using SubmissionTypeEntity = RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;
using SubmissionSubTypeEntity =
    RegOS.ReferenceData.Domain.SubmissionSubType.SubmissionSubType;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SubmissionConfiguration
    : IEntityTypeConfiguration<SubmissionAggregate>
{
    public void Configure(EntityTypeBuilder<SubmissionAggregate> builder)
    {
        builder.ToTable("Submissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => SubmissionId.From(value));

        builder.Property(x => x.ApplicationId)
            .HasConversion(
                id => id.Value,
                value => new RegulatoryApplicationId(value))
            .IsRequired();

        // The published blueprint version this submission is judged against.
        // Nullable: application types with no published template (devices today)
        // are created unbound.
        builder.Property(x => x.BoundTemplateVersionId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new RegulatoryTemplateVersionId(value.Value)
                    : (RegulatoryTemplateVersionId?)null);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // What this filing is rendered as (ADR-047). Not nullable: every
        // submission has a format from the moment it is created, unlike the
        // DTD and gateway metadata that only becomes true at publication and
        // is therefore not modelled here at all.
        builder.Property(x => x.Format)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // PublishedAt is not mapped: it is the RecordedOnUtc of the Published
        // entry in the history below, and a stored copy could disagree with the
        // record beside it (ADR-046).

        // What this was filed as. Null until published (ADR-044 decision 4).
        builder.Property(x => x.SequenceNumber);

        // The regulatory activity (EPIC-007a S003). Three columns, and each null
        // means something different:
        //
        //   OriginatingSubmissionId null -> this sequence OPENS an activity
        //   SubmissionTypeId        null -> this sequence CONTINUES one
        //   SubmissionSubTypeId     null -> this sequence PREDATES S003
        //
        // Only the third is a gap, and it is one nothing can close: sub-type is
        // not derivable from an activity's shape (evidence E13), so the rows
        // that existed before this story keep an honest absence rather than an
        // invented value.
        builder.Property(x => x.OriginatingSubmissionId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? SubmissionId.From(value.Value)
                    : (SubmissionId?)null);

        builder.Property(x => x.SubmissionTypeId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new SubmissionTypeId(value.Value)
                    : (SubmissionTypeId?)null);

        builder.Property(x => x.SubmissionSubTypeId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new SubmissionSubTypeId(value.Value)
                    : (SubmissionSubTypeId?)null);

        // IsClassified is not mapped: it is SubmissionSubTypeId IS NOT NULL, and
        // a stored copy could disagree with the column beside it.
        builder.Ignore(x => x.IsClassified);

        // A submission points at the one that opened its activity — a
        // self-reference within the same table. Restrict, and deliberately:
        // this FK is optional, and EF's default for an optional FK is
        // ClientSetNull, which would silently sever a continuing sequence from
        // its origin instead of refusing (the reference-type-id hazard CLAUDE.md
        // names, arriving here as a delete behaviour rather than as a
        // requiredness one).
        builder.HasOne<SubmissionAggregate>()
            .WithMany()
            .HasForeignKey(x => x.OriginatingSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SubmissionTypeEntity>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SubmissionSubTypeEntity>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionSubTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // "Every sequence in this activity" is the query that renders a package,
        // so it gets an index rather than a scan.
        builder.HasIndex(x => x.OriginatingSubmissionId);
        builder.HasIndex(x => x.SubmissionTypeId);
        builder.HasIndex(x => x.SubmissionSubTypeId);

        // The exclusive-or, in the only place that governs data rather than
        // code. SubmissionClassification already makes a violation
        // unconstructible in C#, but a migration or a hand-written UPDATE never
        // passes through C# — the same division of labour that puts contiguity
        // in the aggregate and uniqueness in an index (ADR-044 decision 6).
        //
        // Three states are legal and no fourth is:
        //   unclassified  — all three null; a sequence filed before S003
        //   opens         — a type, no origin
        //   continues     — an origin, no type
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Submissions_ActivityClassification",
            """
            ("SubmissionSubTypeId" IS NULL
                 AND "SubmissionTypeId" IS NULL
                 AND "OriginatingSubmissionId" IS NULL)
            OR ("SubmissionSubTypeId" IS NOT NULL
                 AND (("SubmissionTypeId" IS NULL)
                       <> ("OriginatingSubmissionId" IS NULL)))
            """));

        // Cross-aggregate foreign keys. The domain exposes no navigation
        // properties, but EF still models the relationships.
        //
        // Application: an Application with Submissions must never be
        // deleted out from under them.
        builder.HasOne<RegulatoryApplicationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        // The bound blueprint version. A deliberate cross-context reference to
        // a child entity of the RegulatoryTemplate aggregate rather than to its
        // root: the *version* is the immutable governance artifact, and naming
        // it here removes the "which version?" question from every later read
        // (see the epic's ADR). Restrict — a version a submission was judged
        // against must never be deleted out from under it.
        builder.HasOne<RegulatoryTemplateVersion>()
            .WithMany()
            .HasForeignKey(x => x.BoundTemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Secondary indexes on the cross-aggregate references.
        // The owning tenant (ADR-031), copied from the parent application at
        // creation. Held by value, no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.BoundTemplateVersionId);

        // Two submissions in one application can never be filed under the same
        // sequence number. This is the *authority* on that, not the numbering
        // policy: the policy reads a number that a concurrent publish may take
        // before the reader saves, and only the index can arbitrate that race
        // (ADR-044 decision 6). SubmissionRepository translates its violation.
        //
        // Filtered, because unlimited drafts legitimately share "no number".
        builder.HasIndex(x => new { x.ApplicationId, x.SequenceNumber })
            .IsUnique()
            .HasFilter("\"SequenceNumber\" IS NOT NULL");

        // Ownership: Submission (1) -> SubmissionDocuments (N). The child has
        // no FK property, so EF uses a shadow "SubmissionId". Deleting a
        // submission deletes its attachments.
        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(SubmissionAggregate.Documents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ownership: Submission (1) -> SubmissionDeletions (N). What this filing
        // withdrew from the sequence before it — written at publish, never after.
        builder.HasMany(x => x.Deletions)
            .WithOne()
            .HasForeignKey("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(SubmissionAggregate.Deletions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ownership: Submission (1) -> SubmissionRoles (N). Who was named on
        // this filing (ADR-048). Cascade, like the other children: the naming
        // has no meaning apart from the filing it appears on.
        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(SubmissionAggregate.Roles))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // The seventh append-only history in RegOS, and the fifth written as an
        // OwnsMany block — the threshold ADR-042 named. The mapping is shared
        // now (ADR-046 decision 6); the entry type and its rules are not.
        builder.OwnsStatusHistory(
            x => x.History,
            "SubmissionStatusEntries",
            "SubmissionId",
            (SubmissionStatusEntryId id) => id.Value,
            value => SubmissionStatusEntryId.From(value),
            SubmissionStatusEntry.NoteMaxLength);
    }
}
