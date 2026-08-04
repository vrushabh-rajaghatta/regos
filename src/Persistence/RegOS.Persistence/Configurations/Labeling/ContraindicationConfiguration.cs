using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// Who must not be given this product here.
/// </summary>
/// <remarks>
/// <b>No status history table</b>, and that is the design rather than an
/// omission: a contraindication is content inside an approved label, so its
/// history is the <c>LocalLabelRevision</c> that published it.
/// </remarks>
public sealed class ContraindicationConfiguration
    : IEntityTypeConfiguration<Contraindication>
{
    public void Configure(EntityTypeBuilder<Contraindication> builder)
    {
        builder.ToTable("Contraindications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ContraindicationId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.LabelText)
            .HasMaxLength(Contraindication.LabelTextMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.OwnsOne(x => x.Condition, concept =>
        {
            CodedConceptColumns.Of<Contraindication>("Condition")(concept);

            // "Which markets contraindicate X?" — the same backwards read the
            // coded condition exists for.
            concept.HasIndex(x => x.Code);
        });

        builder.Navigation(x => x.Condition).IsRequired();

        // The shared mapping, second of three identical calls.
        builder.OwnsMany(
            x => x.Populations,
            populations => ClinicalStatementConfiguration.Populations(
                populations,
                "ContraindicationPopulations",
                "ContraindicationId"));

        builder.Metadata
            .FindNavigation(nameof(Contraindication.Populations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);
    }
}
