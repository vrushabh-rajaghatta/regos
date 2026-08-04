using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Persistence.Configurations.Labeling;

public sealed class GlobalLabelVersionConfiguration
    : IEntityTypeConfiguration<GlobalLabelVersion>
{
    public void Configure(EntityTypeBuilder<GlobalLabelVersion> builder)
    {
        builder.ToTable("GlobalLabelVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new GlobalLabelVersionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // The content link (ADR-059 §6). A plain nullable column holding another
        // context's id — no navigation, and no foreign key: ProductDocument owns
        // that record's lifecycle, and a database-level constraint here would
        // make Labeling's schema a party to it.
        builder.Property(x => x.ContentId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new ProductDocumentId(value.Value)
                    : null);

        builder.Property(x => x.ChangeSummary)
            .HasMaxLength(GlobalLabelVersion.ChangeSummaryMaxLength);

        // Calendar facts, not instants: a label takes effect on a date in the
        // business's world, and the publish is the only thing with a clock time.
        builder.Property(x => x.EffectiveFrom)
            .HasColumnType("date");

        builder.Property(x => x.EffectiveTo)
            .HasColumnType("date");

        builder.Property(x => x.PublishedOnUtc);

        // Shadow FK to the owning label — the child holds no FK property.
        // Declared here and required, because EF's inferred shadow key is
        // NULLABLE by default: an optional FK severs on delete instead of
        // cascading, so a deleted label would leave its versions behind as
        // orphans. It also matters for the unique index below — Postgres treats
        // NULLs as distinct, so a nullable column would let unlimited
        // label-less duplicates past it. Same fix, same reason, as TradeName.
        builder.Property<GlobalLabelId>("GlobalLabelId")
            .HasConversion(id => id.Value, value => new GlobalLabelId(value))
            .IsRequired();

        // One issue number per label, enforced where a race between two
        // concurrent "start a draft" requests cannot slip past the aggregate's
        // own numbering.
        builder.HasIndex("GlobalLabelId", nameof(GlobalLabelVersion.VersionNumber))
            .IsUnique();

        // "Which version is in force?" — asked on every read of a label.
        builder.HasIndex("GlobalLabelId", nameof(GlobalLabelVersion.Status));
    }
}
