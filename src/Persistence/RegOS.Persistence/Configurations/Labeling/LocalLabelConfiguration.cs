using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Labeling;

public sealed class LocalLabelConfiguration
    : IEntityTypeConfiguration<LocalLabel>
{
    public void Configure(EntityTypeBuilder<LocalLabel> builder)
    {
        builder.ToTable("LocalLabels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new LocalLabelId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        // Stored as the two-letter code the value object validated. Reading it
        // back goes through FromIso639_1, so a column someone edited by hand
        // still cannot become an invalid LanguageCode.
        builder.Property(x => x.Language)
            .HasConversion(
                language => language.Value,
                value => LanguageCode.FromIso639_1(value))
            .HasMaxLength(LanguageCode.Length)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.OwnsOne(x => x.LabelType, concept =>
        {
            concept.Property(x => x.System)
                .HasColumnName("LabelTypeSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            concept.Property(x => x.Code)
                .HasColumnName("LabelTypeCode")
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            concept.Property(x => x.Display)
                .HasColumnName("LabelTypeDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();
        });

        builder.Navigation(x => x.LabelType).IsRequired();

        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey("LocalLabelId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(LocalLabel.Revisions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Cross-context foreign key; the domain exposes no navigation property.
        // Cascade, not Restrict: a market's own labelling has no meaning apart
        // from the market, unlike a global label, which survives any one
        // jurisdiction.
        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // The pack this document is printed for, when it is printed for one
        // (EPIC-010b D6 — the debt EPIC-018 named this epic as the milestone
        // for). Nullable on every label and not only on artwork: no rule may
        // read `if (Type == Artwork)`, and a container label is genuinely
        // printed per pack size.
        //
        // SetNull rather than Cascade: deleting a pack must not take an
        // authority-approved document with it. The artwork outlives the pack
        // record and simply stops naming one.
        builder.Property(x => x.PackagedProductId)
            .HasConversion(
                id => id!.Value, value => new PackagedProductId(value));

        builder.HasOne<PackagedProduct>()
            .WithMany()
            .HasForeignKey(x => x.PackagedProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // "What labelling do we hold for this market?" — the only question that
        // reaches this table today.
        builder.HasIndex(x => x.MedicinalProductId);

        // "Which artwork is printed for this pack?" — the capstone's question,
        // asked from the pack's side.
        builder.HasIndex(x => x.PackagedProductId);

        builder.HasIndex(x => x.TenantId);

        // Deliberately NOT unique on (MedicinalProductId, LabelTypeCode,
        // Language). A market may hold two leaflets in one language where the
        // audiences differ, and a carton family may carry several artworks.
        // Uniqueness we cannot justify is uniqueness that will be wrong for
        // somebody — the call GlobalLabel and MedicinalProduct both made.
    }
}
