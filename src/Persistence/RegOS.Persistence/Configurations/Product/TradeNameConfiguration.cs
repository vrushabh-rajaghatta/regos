using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Persistence.Configurations.Product;

public sealed class TradeNameConfiguration
    : IEntityTypeConfiguration<TradeName>
{
    public void Configure(EntityTypeBuilder<TradeName> builder)
    {
        builder.ToTable("TradeNames");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TradeNameId(value));

        // Stored as the two-letter code the value object validated. Reading it
        // back goes through FromIso639_1 rather than a constructor, so a column
        // someone edited by hand still cannot become an invalid LanguageCode.
        builder.Property(x => x.Language)
            .HasConversion(
                language => language.Value,
                value => LanguageCode.FromIso639_1(value))
            .HasMaxLength(LanguageCode.Length)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(TradeName.NameMaxLength)
            .IsRequired();

        // Shadow FK to the owning market — the child holds no FK property.
        // Required: a trade name with no market is not a weaker record, it is
        // an orphan. It also matters for the index below — Postgres treats
        // NULLs as distinct, so a nullable column would let unlimited
        // market-less duplicates past a UNIQUE constraint.
        builder.Property<MedicinalProductId>("MedicinalProductId")
            .HasConversion(
                id => id.Value,
                value => new MedicinalProductId(value))
            .IsRequired();

        // The same rule the aggregate enforces, so a race between two
        // concurrent requests cannot slip a second English name past it.
        //
        // Deliberately unique, and deliberately the OPPOSITE of the index one
        // tier up, where (GlobalProductId, CountryId) is *not* unique. Two
        // market presences in one country are two business objects a company
        // may legitimately hold; two English names for one market presence are
        // two labels for one thing, so one of them is wrong.
        builder
            .HasIndex("MedicinalProductId", nameof(TradeName.Language))
            .IsUnique();
    }
}
