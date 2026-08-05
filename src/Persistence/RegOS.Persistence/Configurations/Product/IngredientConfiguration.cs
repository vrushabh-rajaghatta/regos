using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Persistence.Configurations.Product;

public sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("Ingredients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new IngredientId(value))
            .ValueGeneratedNever();

        // No TenantId: an ingredient is reachable only through a filtered root
        // (ADR-031), the same as TradeName one tier up.
        //
        // Ownership declared from this side rather than the presentation's, so
        // the shadow foreign key and the unique index that names it are written
        // in one place and in order. Entity type configurations run in no
        // guaranteed order, and the index cannot name a column its own file
        // does not create.
        //
        // The key stays a shadow property. Declaring it explicitly as a Guid
        // does not match the principal's typed key, and declaring it as
        // PharmaceuticalProductDetailId would make it *optional* — and an
        // optional foreign key severs instead of deleting.
        // IsRequired is load-bearing, not decoration. A shadow foreign key is
        // nullable by default, and a nullable one lets the database hold an
        // ingredient that belongs to no presentation — the "optional FK severs
        // instead of deleting" trap the identity conventions call out.
        builder.HasOne<PharmaceuticalProductDetail>()
            .WithMany(x => x.Ingredients)
            .HasForeignKey("PharmaceuticalProductDetailId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // One row per substance per composition. The aggregate checks it too;
        // this is what a race between two concurrent requests hits.
        builder.HasIndex("PharmaceuticalProductDetailId", "SubstanceId")
            .IsUnique();

        builder.Property(x => x.SubstanceId)
            .HasConversion(id => id.Value, value => new SubstanceId(value))
            .IsRequired();

        // Where this substance comes from (ADR-063 §2). Nullable, and it will
        // stay mostly null: RegOS holds no provenance for any ingredient
        // recorded before EPIC-010c S003, and absent means "nobody has said"
        // rather than "unsourced".
        //
        // No foreign key, and that is the decision. Ingredient is a child of
        // PharmaceuticalProductDetail with no TenantId of its own, reachable
        // only through a filtered root — while OrganizationSite is a root in
        // another context with its own filter. A cascade or restrict between
        // them would tie a composition's lifetime to a registry row's, and a
        // restrict in particular would refuse to deactivate a site because a
        // formulation once named it. The id is held by value, the same way
        // TenantId is (ADR-031).
        builder.Property(x => x.ManufacturingSourceSiteId)
            .HasConversion(
                id => id!.Value, value => new OrganizationSiteId(value));

        // A rule branches on this, so it is stored as its name rather than an
        // ordinal — a reader of the table can see which rows are actives, and
        // reordering the enum cannot silently reclassify a formulation.
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Optional as a whole: an excipient's quantity is routinely undeclared,
        // so all four columns are nullable together. An active without one is
        // refused by the aggregate, not by the schema — the rule depends on the
        // role, which a check constraint would have to duplicate.
        builder.OwnsOne(x => x.Strength, strength =>
        {
            strength.Property(s => s.NumeratorValue)
                .HasColumnName("StrengthNumeratorValue")
                .HasPrecision(18, 6);

            strength.OwnsOne(s => s.NumeratorUnit, unit =>
            {
                unit.Property(u => u.System)
                    .HasColumnName("StrengthNumeratorUnitSystem")
                    .HasMaxLength(CodedConcept.SystemMaxLength);

                unit.Property(u => u.Code)
                    .HasColumnName("StrengthNumeratorUnitCode")
                    .HasMaxLength(CodedConcept.CodeMaxLength);

                unit.Property(u => u.Display)
                    .HasColumnName("StrengthNumeratorUnitDisplay")
                    .HasMaxLength(CodedConcept.DisplayMaxLength);
            });

            strength.Property(s => s.DenominatorValue)
                .HasColumnName("StrengthDenominatorValue")
                .HasPrecision(18, 6);

            strength.OwnsOne(s => s.DenominatorUnit, unit =>
            {
                unit.Property(u => u.System)
                    .HasColumnName("StrengthDenominatorUnitSystem")
                    .HasMaxLength(CodedConcept.SystemMaxLength);

                unit.Property(u => u.Code)
                    .HasColumnName("StrengthDenominatorUnitCode")
                    .HasMaxLength(CodedConcept.CodeMaxLength);

                unit.Property(u => u.Display)
                    .HasColumnName("StrengthDenominatorUnitDisplay")
                    .HasMaxLength(CodedConcept.DisplayMaxLength);
            });
        });

        // Cross-aggregate foreign key; the domain exposes no navigation
        // property. Restrict, not Cascade: a substance is a fact in its own
        // right, and deleting one out from under a formulation would silently
        // change what a product is made of.
        builder.HasOne<Substance>()
            .WithMany()
            .HasForeignKey(x => x.SubstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        // "Which products contain substance X?" — the epic's whole reason to
        // exist, answered off one index rather than a scan.
        builder.HasIndex(x => x.SubstanceId);
    }
}
