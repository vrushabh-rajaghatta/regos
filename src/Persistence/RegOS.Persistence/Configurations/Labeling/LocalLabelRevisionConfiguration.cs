using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Persistence.Configurations.Labeling;

public sealed class LocalLabelRevisionConfiguration
    : IEntityTypeConfiguration<LocalLabelRevision>
{
    public void Configure(EntityTypeBuilder<LocalLabelRevision> builder)
    {
        builder.ToTable("LocalLabelRevisions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new LocalLabelRevisionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.RevisionNumber)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // The approved document (ADR-059 §6). A plain nullable column holding
        // another context's id — no navigation and no foreign key, because
        // ProductDocument owns that record's lifecycle.
        builder.Property(x => x.ContentId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new ProductDocumentId(value.Value)
                    : null);

        // The core version this was written from. Nullable by decision, not by
        // omission (EPIC-018 D3) — and a real FK, because both tiers live in
        // this context and a dangling derivation would be a lie.
        builder.Property(x => x.DerivedFromGlobalLabelVersionId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new GlobalLabelVersionId(value.Value)
                    : null);

        builder.HasOne<GlobalLabelVersion>()
            .WithMany()
            .HasForeignKey(x => x.DerivedFromGlobalLabelVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.DataCarrierCode)
            .HasMaxLength(LocalLabelRevision.DataCarrierCodeMaxLength);

        builder.Property(x => x.ChangeSummary)
            .HasMaxLength(LocalLabelRevision.ChangeSummaryMaxLength);

        // Calendar facts in a jurisdiction, not instants. Approval and effect
        // are separate columns because they are separate regulatory facts.
        builder.Property(x => x.ApprovedOn).HasColumnType("date");
        builder.Property(x => x.EffectiveFrom).HasColumnType("date");
        builder.Property(x => x.EffectiveTo).HasColumnType("date");

        // Shadow FK to the owning label, declared and required. EF's inferred
        // shadow key is nullable, an optional FK severs instead of cascading,
        // and Postgres treats NULLs as distinct — so the unique index below
        // would stop constraining parentless rows. Enforced by
        // AggregateChildArchitectureTests.
        builder.Property<LocalLabelId>("LocalLabelId")
            .HasConversion(id => id.Value, value => new LocalLabelId(value))
            .IsRequired();

        // One revision number per label, enforced where a race between two
        // concurrent "start a revision" requests cannot slip past the
        // aggregate's own numbering.
        builder.HasIndex("LocalLabelId", nameof(LocalLabelRevision.RevisionNumber))
            .IsUnique();

        // "What does this market's label say today?" — asked on every read.
        builder.HasIndex("LocalLabelId", nameof(LocalLabelRevision.Status));

        // "Which markets have adopted core version 7?" — the question the
        // derivation link exists to answer, and the reason it is indexed.
        builder.HasIndex(x => x.DerivedFromGlobalLabelVersionId);
    }
}
