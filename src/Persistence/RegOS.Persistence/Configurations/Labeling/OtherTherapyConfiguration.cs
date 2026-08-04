using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Persistence.Configurations.Labeling;

public sealed class OtherTherapyConfiguration
    : IEntityTypeConfiguration<OtherTherapy>
{
    public void Configure(EntityTypeBuilder<OtherTherapy> builder)
    {
        builder.ToTable("IndicationOtherTherapies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new OtherTherapyId(value))
            .ValueGeneratedNever();

        // Free text, deliberately: a drug class or a procedure is not a
        // Substance, and a required link would make two of the three cases
        // unrecordable.
        builder.Property(x => x.Therapy)
            .HasMaxLength(OtherTherapy.TherapyMaxLength)
            .IsRequired();

        builder.OwnsOne(
            x => x.Relationship,
            IndicationConfiguration.ConceptColumns<OtherTherapy>("Relationship"));

        builder.Navigation(x => x.Relationship).IsRequired();

        builder.Property<IndicationId>("IndicationId")
            .HasConversion(id => id.Value, value => new IndicationId(value))
            .IsRequired();

        builder.HasIndex("IndicationId");
    }
}
