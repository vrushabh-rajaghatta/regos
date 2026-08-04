using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// A side effect this market's approved label lists, and how often it occurs.
/// </summary>
/// <remarks>
/// <b>The one column the three statement types do not share is
/// <c>Frequency</c></b> — three nullable columns via the same coded-value
/// helper. It is an attribute, not an invariant, which is why it argued against
/// a shared domain type and not against the shared population mapping below.
/// </remarks>
public sealed class UndesirableEffectConfiguration
    : IEntityTypeConfiguration<UndesirableEffect>
{
    public void Configure(EntityTypeBuilder<UndesirableEffect> builder)
    {
        builder.ToTable("UndesirableEffects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new UndesirableEffectId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.LabelText)
            .HasMaxLength(UndesirableEffect.LabelTextMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.OwnsOne(x => x.Effect, concept =>
        {
            CodedConceptColumns.Of<UndesirableEffect>("Effect")(concept);

            concept.HasIndex(x => x.Code);
        });

        builder.Navigation(x => x.Effect).IsRequired();

        // Nullable: a label may list an effect without stating a band, and
        // "not known" is itself one.
        builder.OwnsOne(
            x => x.Frequency,
            CodedConceptColumns.Of<UndesirableEffect>(
                "Frequency", required: false));

        // The shared mapping, third of three identical calls.
        builder.OwnsMany(
            x => x.Populations,
            populations => ClinicalStatementConfiguration.Populations(
                populations,
                "UndesirableEffectPopulations",
                "UndesirableEffectId"));

        builder.Metadata
            .FindNavigation(nameof(UndesirableEffect.Populations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);
    }
}
