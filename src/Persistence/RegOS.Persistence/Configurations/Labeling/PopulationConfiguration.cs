using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// Who a clinical statement applies to.
/// </summary>
/// <remarks>
/// <b>Its own table, owned by exactly one statement</b> (EPIC-018 D2). S004 adds
/// the same <em>shape</em> to contraindications and undesirable effects, and
/// each will get its own table for the reason this one exists: an owned value is
/// tracked against a single owner, and a shared table would need a discriminator
/// and would lose the foreign key that makes the relationship real.
/// <para>
/// The column mapping is centralised in
/// <see cref="IndicationConfiguration.ConceptColumns{TOwner}"/> rather than
/// copied — which is where the founder's D2 caveat lands: share the EF helper,
/// never a domain base type across four roots.
/// </para>
/// </remarks>
public sealed class PopulationConfiguration
    : IEntityTypeConfiguration<Population>
{
    public void Configure(EntityTypeBuilder<Population> builder)
    {
        builder.ToTable("IndicationPopulations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PopulationId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.AgeLow);
        builder.Property(x => x.AgeHigh);

        builder.Property(x => x.Description)
            .HasMaxLength(Population.DescriptionMaxLength);

        builder.OwnsOne(
            x => x.AgeUnit,
            IndicationConfiguration.ConceptColumns<Population>(
                "AgeUnit", required: false));

        builder.OwnsOne(
            x => x.Gender,
            IndicationConfiguration.ConceptColumns<Population>("Gender"));

        builder.Navigation(x => x.Gender).IsRequired();

        builder.OwnsOne(
            x => x.PhysiologicalCondition,
            IndicationConfiguration.ConceptColumns<Population>(
                "PhysiologicalCondition", required: false));

        // Shadow FK to the owning statement, declared and required — EF's
        // inferred one is nullable, and an optional FK severs instead of
        // cascading. Enforced by AggregateChildArchitectureTests.
        builder.Property<IndicationId>("IndicationId")
            .HasConversion(id => id.Value, value => new IndicationId(value))
            .IsRequired();

        builder.HasIndex("IndicationId");
    }
}
