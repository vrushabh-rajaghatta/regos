using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Labeling;

public sealed class GlobalLabelConfiguration
    : IEntityTypeConfiguration<GlobalLabel>
{
    public void Configure(EntityTypeBuilder<GlobalLabel> builder)
    {
        builder.ToTable("GlobalLabels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new GlobalLabelId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.GlobalProductId)
            .HasConversion(id => id.Value, value => new GlobalProductId(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(GlobalLabel.NameMaxLength)
            .IsRequired();

        // There is deliberately no Status column. A label's meaningful lifecycle
        // lives in its versions; "retire this label" is a capability nobody has
        // asked for, and a column that is always Active is a field nobody filled
        // in. Same call Substance made on IsActive (EPIC-010a S001).
        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // Three columns rather than a foreign key: the vocabulary is code, not
        // data, and the System column is what makes replacing it a migration.
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

        // Ownership: label (1) -> versions (N). The child holds no FK property,
        // so EF uses a shadow "GlobalLabelId".
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("GlobalLabelId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(GlobalLabel.Versions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Cross-context foreign key; the domain exposes no navigation property.
        // Restrict, not Cascade: a product with labels held against it is not a
        // product anyone should be able to delete out from under them.
        builder.HasOne<GlobalProduct>()
            .WithMany()
            .HasForeignKey(x => x.GlobalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // "What labels do we hold for this product?" — the only question that
        // reaches this table today.
        builder.HasIndex(x => x.GlobalProductId);

        builder.HasIndex(x => x.TenantId);

        // Deliberately NOT unique on (GlobalProductId, LabelTypeCode). A company
        // may hold two patient leaflets for one product where the audiences
        // differ, and uniqueness we cannot justify is uniqueness that will be
        // wrong for somebody — the call MedicinalProduct made on
        // (GlobalProductId, CountryId), for the same reason.
    }
}
