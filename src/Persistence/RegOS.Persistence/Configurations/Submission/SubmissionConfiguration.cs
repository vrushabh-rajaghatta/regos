using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using SubmissionTypeAggregate = RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

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

        builder.Property(x => x.SubmissionTypeId)
            .HasConversion(
                id => id.Value,
                value => new SubmissionTypeId(value))
            .IsRequired();

        // The published blueprint version this submission is judged against.
        // Nullable: submission types with no published template (devices today)
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

        builder.Property(x => x.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // PublishedAt is not mapped: it is the RecordedOnUtc of the Published
        // entry in the history below, and a stored copy could disagree with the
        // record beside it (ADR-046).

        // What this was filed as. Null until published (ADR-044 decision 4).
        builder.Property(x => x.SequenceNumber);

        // Cross-aggregate foreign keys. The domain exposes no navigation
        // properties, but EF still models the relationships.
        //
        // Application: an Application with Submissions must never be
        // deleted out from under them.
        builder.HasOne<RegulatoryApplicationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        // SubmissionType is reference data. Historical Submissions stay
        // valid even when a type is deactivated — inactive, not deleted.
        builder.HasOne<SubmissionTypeAggregate>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionTypeId)
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
        builder.HasIndex(x => x.SubmissionTypeId);
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

        // The seventh append-only history in RegOS, and the fifth written as an
        // OwnsMany block — which is the threshold ADR-042 named for extracting
        // the *configuration* (not the entry type, and not the behaviour). See
        // the epic's S003 note for the measurement.
        builder.OwnsMany(x => x.History, entry =>
        {
            entry.ToTable("SubmissionStatusEntries");

            entry.WithOwner().HasForeignKey("SubmissionId");

            entry.HasKey(x => x.Id);

            entry.Property(x => x.Id)
                .HasColumnName("Id")
                .HasConversion(
                    id => id.Value, value => SubmissionStatusEntryId.From(value));

            entry.Property(x => x.Status).HasConversion<int>().IsRequired();
            entry.Property(x => x.OccurredOn).IsRequired();
            entry.Property(x => x.RecordedOnUtc).IsRequired();

            entry.Property(x => x.Note)
                .HasMaxLength(SubmissionStatusEntry.NoteMaxLength);

            entry.HasIndex("SubmissionId");
        });

        builder.Navigation(x => x.History).AutoInclude();
    }
}
