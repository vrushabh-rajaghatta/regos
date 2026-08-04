using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// What this product clashes with here.
/// </summary>
/// <remarks>
/// The fourth call to <see cref="ClinicalStatementConfiguration.Populations"/>,
/// with the same two strings and nothing else — the helper's justification held
/// for one more owner than it was extracted on.
/// </remarks>
public sealed class DrugInteractionConfiguration
    : IEntityTypeConfiguration<DrugInteraction>
{
    public void Configure(EntityTypeBuilder<DrugInteraction> builder)
    {
        builder.ToTable("Interactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DrugInteractionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.LabelText)
            .HasMaxLength(DrugInteraction.LabelTextMaxLength)
            .IsRequired();

        builder.Property(x => x.Management)
            .HasMaxLength(DrugInteraction.ManagementMaxLength);

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.OwnsOne(
            x => x.InteractionType,
            CodedConceptColumns.Of<DrugInteraction>("InteractionType"));

        builder.Navigation(x => x.InteractionType).IsRequired();

        // Nullable: many labels describe an interaction without grading it.
        builder.OwnsOne(
            x => x.Severity,
            CodedConceptColumns.Of<DrugInteraction>("Severity", required: false));

        builder.OwnsMany(x => x.Interactants, interactants =>
        {
            interactants.ToTable("Interactants");

            interactants.WithOwner().HasForeignKey("DrugInteractionId");

            interactants.HasKey(x => x.Id);

            interactants.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new InteractantId(value))
                .ValueGeneratedNever();

            interactants.Property(x => x.Description)
                .HasMaxLength(Interactant.DescriptionMaxLength)
                .IsRequired();

            // The seam, and a real foreign key: both live in tables this schema
            // owns, so a dangling reference would be a lie the database could
            // have prevented. Restrict, not Cascade — deleting a substance must
            // not silently rewrite what a label says.
            interactants.Property(x => x.SubstanceId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value != null ? new SubstanceId(value.Value) : null);

            interactants.HasOne<Substance>()
                .WithMany()
                .HasForeignKey(x => x.SubstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // "Which of our products interact with warfarin?" — the backwards
            // read the optional link exists for.
            interactants.HasIndex(x => x.SubstanceId);

            interactants.HasIndex("DrugInteractionId");
        });

        builder.Metadata
            .FindNavigation(nameof(DrugInteraction.Interactants))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(
            x => x.Populations,
            populations => ClinicalStatementConfiguration.Populations(
                populations, "InteractionPopulations", "DrugInteractionId"));

        builder.Metadata
            .FindNavigation(nameof(DrugInteraction.Populations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);
    }
}
