using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Product;

public sealed class PharmaceuticalProductDetailConfiguration
    : IEntityTypeConfiguration<PharmaceuticalProductDetail>
{
    public void Configure(EntityTypeBuilder<PharmaceuticalProductDetail> builder)
    {
        builder.ToTable("PharmaceuticalProductDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new PharmaceuticalProductDetailId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(PharmaceuticalProductDetail.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(PharmaceuticalProductDetail.DescriptionMaxLength);

        builder.OwnsOne(x => x.DoseForm, concept =>
        {
            concept.Property(x => x.System)
                .HasColumnName("DoseFormSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            concept.Property(x => x.Code)
                .HasColumnName("DoseFormCode")
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            concept.Property(x => x.Display)
                .HasColumnName("DoseFormDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();
        });

        builder.Navigation(x => x.DoseForm).IsRequired();

        // Optional: an oral solution measured in mL has no article to count,
        // so all three columns are nullable together.
        builder.OwnsOne(x => x.UnitOfPresentation, concept =>
        {
            concept.Property(x => x.System)
                .HasColumnName("UnitOfPresentationSystem")
                .HasMaxLength(CodedConcept.SystemMaxLength);

            concept.Property(x => x.Code)
                .HasColumnName("UnitOfPresentationCode")
                .HasMaxLength(CodedConcept.CodeMaxLength);

            concept.Property(x => x.Display)
                .HasColumnName("UnitOfPresentationDisplay")
                .HasMaxLength(CodedConcept.DisplayMaxLength);
        });

        // A table rather than columns, because the count is genuinely open —
        // a solution for injection is routinely intravenous and intramuscular.
        // Modelled once, here, and not also as a standalone object (D6).
        builder.OwnsMany(x => x.RoutesOfAdministration, route =>
        {
            route.ToTable("PharmaceuticalProductRoutes");

            route.WithOwner().HasForeignKey("PharmaceuticalProductDetailId");

            route.Property(x => x.System)
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            route.Property(x => x.Code)
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            route.Property(x => x.Display)
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();

            // One route per presentation, enforced where a race cannot slip
            // past the aggregate's own check.
            route.HasIndex("PharmaceuticalProductDetailId", "Code").IsUnique();
        });

        builder.Metadata
            .FindNavigation(nameof(PharmaceuticalProductDetail.RoutesOfAdministration))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // What it looks like, as one owned value object.
        //
        // Required, for the reason PackagedProduct.ShelfLife is (EPIC-010b
        // S003): EF reads an *optional* owned reference back as null when every
        // column it shares is null, and a presentation whose only statement is
        // "white" has exactly that — its colours would vanish on reload while
        // the write succeeded. PhysicalCharacteristics.NotStated is the empty
        // value that makes a required navigation honest.
        builder.OwnsOne(x => x.Appearance, appearance =>
        {
            appearance.OwnsOne(x => x.Shape, shape =>
            {
                shape.Property(x => x.System)
                    .HasColumnName("AppearanceShapeSystem")
                    .HasMaxLength(CodedConcept.SystemMaxLength);

                shape.Property(x => x.Code)
                    .HasColumnName("AppearanceShapeCode")
                    .HasMaxLength(CodedConcept.CodeMaxLength);

                shape.Property(x => x.Display)
                    .HasColumnName("AppearanceShapeDisplay")
                    .HasMaxLength(CodedConcept.DisplayMaxLength);
            });

            appearance.Property(x => x.Imprint)
                .HasColumnName("AppearanceImprint")
                .HasMaxLength(PhysicalCharacteristics.ImprintMaxLength);

            appearance.Property(x => x.Description)
                .HasColumnName("AppearanceDescription")
                .HasMaxLength(PhysicalCharacteristics.DescriptionMaxLength);

            // A table rather than columns: a capsule with a white body and a
            // blue cap is two colours, and that is ordinary rather than
            // exceptional.
            appearance.OwnsMany(x => x.Colours, colour =>
            {
                colour.ToTable("PharmaceuticalProductColours");

                colour.WithOwner().HasForeignKey("PharmaceuticalProductDetailId");

                colour.Property(x => x.System)
                    .HasMaxLength(CodedConcept.SystemMaxLength)
                    .IsRequired();

                colour.Property(x => x.Code)
                    .HasMaxLength(CodedConcept.CodeMaxLength)
                    .IsRequired();

                colour.Property(x => x.Display)
                    .HasMaxLength(CodedConcept.DisplayMaxLength)
                    .IsRequired();

                colour.HasIndex("PharmaceuticalProductDetailId", "Code").IsUnique();
            });
        });

        builder.Navigation(x => x.Appearance).IsRequired();

        builder.Metadata
            .FindNavigation(nameof(PharmaceuticalProductDetail.Appearance))!
            .TargetEntityType
            .FindNavigation(nameof(PhysicalCharacteristics.Colours))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ownership: presentation (1) -> ingredients (N). The relationship
        // itself is declared from the Ingredient side, so its shadow foreign
        // key and the unique index that names it stay together; only the
        // navigation's access mode belongs here.
        builder.Metadata
            .FindNavigation(nameof(PharmaceuticalProductDetail.Ingredients))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        // Cross-aggregate foreign key; the domain exposes no navigation
        // property. Cascade, not Restrict: a presentation has no meaning
        // without the market it describes, unlike a licence or a global
        // product, which are facts in their own right.
        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // "What presentations does this market have?" — the only question that
        // reaches this table today, and the one the panel asks on every load.
        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);

        // Deliberately NOT unique on (MedicinalProductId, Name). 10 mg, 20 mg
        // and 40 mg are one commercial presence with three presentations, and
        // nothing regulatory says a market may hold only one. If a rule ever
        // does, it arrives explicitly rather than by assumption.
    }
}
