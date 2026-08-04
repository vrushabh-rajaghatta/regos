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

        // Stored, not replayed — the portfolio views read one column rather
        // than reducing a history per row. A different concept from Status
        // above and stored as a different column, so nothing can read one as
        // the other. Kept as int, matching RegistrationStatus.
        builder.Property(x => x.CurrentMarketStatus)
            .HasConversion<int>()
            .IsRequired();

        // A single nullable column, per EPIC-017's change-case analysis. Stored
        // as the string it is: a value object over a code the tenant supplied,
        // deliberately not a CodedConcept, because RegOS holds no WHO ATC
        // licence to name as its system (ADR-058 §6).
        builder.Property(x => x.AtcCode)
            .HasConversion(
                code => code!.Value,
                value => AtcCode.Create(value))
            .HasMaxLength(AtcCode.MaxLength);

        // "Which of our markets are analgesics?" is a prefix match on this
        // column — the reason AtcCode.Levels is derived rather than a table of
        // parent codes.
        builder.HasIndex(x => x.AtcCode);

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

        // Ownership: MedicinalProduct (1) -> trade names (N). The child holds
        // no FK property, so EF uses a shadow "MedicinalProductId".
        builder.HasMany(x => x.TradeNames)
            .WithOne()
            .HasForeignKey("MedicinalProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(MedicinalProduct.TradeNames))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ownership: MedicinalProduct (1) -> market status history (N).
        builder.HasMany(x => x.MarketStatusHistory)
            .WithOne()
            .HasForeignKey("MedicinalProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(MedicinalProduct.MarketStatusHistory))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // "What is actually on sale in this market?" — the S004 portfolio
        // question, answered off one index rather than a history reduction.
        builder.HasIndex(x => new { x.CountryId, x.CurrentMarketStatus });
    }
}
