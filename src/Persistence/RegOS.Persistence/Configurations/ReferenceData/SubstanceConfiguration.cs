using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class SubstanceConfiguration : IEntityTypeConfiguration<Substance>
{
    public void Configure(EntityTypeBuilder<Substance> builder)
    {
        builder.ToTable("Substances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SubstanceId(value));

        // Null for the shared catalogue, set for a tenant's own —
        // platform-shipped, tenant-extensible (ADR-058 §2).
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id!.Value,
                value => TenantId.From(value));

        builder.Property(x => x.Name)
            .HasMaxLength(Substance.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Inn)
            .HasMaxLength(Substance.NameMaxLength);

        // Inline on the row rather than a joined vocabulary table: the coded
        // value is three short strings, and a lookup table would need the
        // stewardship RegOS has not built (EPIC-012) to hold words it does not
        // own. The columns are named <Concept>System/Code/Display so that when
        // licensed terminology arrives, System is what the migration reads —
        // the seam is visible in the schema, not only in the model.
        builder.OwnsOne(x => x.SubstanceClass, concept =>
        {
            concept.Property(x => x.System)
                .HasColumnName("SubstanceClassSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            concept.Property(x => x.Code)
                .HasColumnName("SubstanceClassCode")
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            concept.Property(x => x.Display)
                .HasColumnName("SubstanceClassDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();
        });

        builder.Navigation(x => x.SubstanceClass).IsRequired();

        builder.OwnsOne(x => x.SubstanceType, concept =>
        {
            concept.Property(x => x.System)
                .HasColumnName("SubstanceTypeSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            concept.Property(x => x.Code)
                .HasColumnName("SubstanceTypeCode")
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            concept.Property(x => x.Display)
                .HasColumnName("SubstanceTypeDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();
        });

        builder.Navigation(x => x.SubstanceType).IsRequired();

        builder.Property(x => x.CasNumber)
            .HasMaxLength(Substance.IdentifierMaxLength);

        builder.Property(x => x.UniiCode)
            .HasMaxLength(Substance.IdentifierMaxLength);

        builder.Property(x => x.MolecularFormula)
            .HasMaxLength(Substance.MolecularFormulaMaxLength);

        builder.Property(x => x.Description)
            .HasMaxLength(Substance.DescriptionMaxLength);

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        // Two tenants may each add a compound the platform did not ship without
        // colliding, and neither can add its own twice. The other half of the
        // rule — a tenant may not shadow a name the shared catalogue already
        // carries — no index can express, and lives in the create path.
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();
    }
}
