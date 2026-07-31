using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Primitives;

using CountryAggregate = RegOS.ReferenceData.Domain.Geography.Country.Country;

namespace RegOS.Persistence.Configurations.Product;

public sealed class MedicinalProductConfiguration
    : IEntityTypeConfiguration<MedicinalProduct>
{
    public void Configure(EntityTypeBuilder<MedicinalProduct> builder)
    {
        builder.ToTable("MedicinalProducts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.GlobalProductId)
            .HasConversion(id => id.Value, value => new GlobalProductId(value))
            .IsRequired();

        builder.Property(x => x.CountryId)
            .HasConversion(id => id.Value, value => new CountryId(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // A calendar fact in a jurisdiction, not an instant.
        builder.Property(x => x.StatusDate)
            .HasColumnType("date")
            .IsRequired();

        // Cross-aggregate foreign keys; the domain exposes no navigation
        // properties. Restrict on both: a global product or a country that a
        // market presence names must never be deleted out from under it.
        builder.HasOne<GlobalProduct>()
            .WithMany()
            .HasForeignKey(x => x.GlobalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CountryAggregate>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TenantId);

        // "Which markets is this product in?" — the query the tier exists for.
        builder.HasIndex(x => new { x.GlobalProductId, x.CountryId });

        // Deliberately NOT unique on (GlobalProductId, CountryId). A medicinal
        // product is identified by its own identity, not by the pair: several
        // may exist for one pair when the business distinguishes them —
        // presentations, strengths, or the two halves of a partial divestment.
        // This is the same call EPIC-005 made for Registration, one tier up,
        // and it is what makes resolve-or-create impossible rather than merely
        // unwise.
    }
}
