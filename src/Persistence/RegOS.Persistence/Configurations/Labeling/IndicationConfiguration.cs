using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Labeling;

public sealed class IndicationConfiguration
    : IEntityTypeConfiguration<Indication>
{
    public void Configure(EntityTypeBuilder<Indication> builder)
    {
        builder.ToTable("Indications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new IndicationId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.LabelText)
            .HasMaxLength(Indication.LabelTextMaxLength)
            .IsRequired();

        // Stored, not replayed — the cross-market views read one indexed column
        // rather than reducing a history per row. Kept as int, matching
        // MedicinalProduct's market status.
        builder.Property(x => x.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // The join key for every cross-market question. Three columns rather
        // than a foreign key: the vocabulary is code, and the System column is
        // what makes replacing it with MedDRA a migration (ADR-058 §3).
        // "Which markets approve indication X?" — the capstone question, and
        // the reason the condition is coded at all. The index lives inside the
        // owned block because the column belongs to the owned type even though
        // it shares the root's table.
        builder.OwnsOne(x => x.Condition, concept =>
        {
            CodedConceptColumns.Of<Indication>("Condition")(concept);

            concept.HasIndex(x => x.Code);
        });

        builder.Navigation(x => x.Condition).IsRequired();

        // The shared mapping, one of three identical calls (EPIC-018 S004).
        builder.OwnsMany(
            x => x.Populations,
            populations => ClinicalStatementConfiguration.Populations(
                populations, "IndicationPopulations", "IndicationId"));

        builder.Metadata
            .FindNavigation(nameof(Indication.Populations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.OtherTherapies)
            .WithOne()
            .HasForeignKey("IndicationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Indication.OtherTherapies))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey("IndicationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Indication.StatusHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Cross-context foreign key; the domain exposes no navigation property.
        // Cascade: an authorisation has no meaning apart from the market that
        // granted it.
        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);

        // Deliberately NOT unique on (MedicinalProductId, ConditionCode). One
        // market may hold two authorisations for one condition where the
        // populations differ — adults and paediatrics approved years apart is
        // the ordinary case, not a duplicate.

        // The other half of the capstone read: the join starts from a filtered
        // root, then narrows to what is actually authorised.
        builder.HasIndex(x => x.CurrentStatus);
    }
}
