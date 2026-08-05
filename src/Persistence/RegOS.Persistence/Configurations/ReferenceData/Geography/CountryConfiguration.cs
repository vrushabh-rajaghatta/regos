using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Persistence.Configurations.ReferenceData.Geography;

public sealed class CountryConfiguration
    : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new CountryId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(2)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        // Exactly three, not "up to three": the domain refuses anything else,
        // and a char(3) column says so to anyone reading the schema instead.
        builder.Property(x => x.IsoAlpha3Code)
            .HasMaxLength(Country.IsoAlpha3CodeLength)
            .IsFixedLength()
            .IsRequired();

        // Unique for the same reason alpha-2 is: it identifies the country to
        // an outside system, and two rows claiming USA is a data defect that
        // would surface as a malformed submission rather than as an error here.
        builder.HasIndex(x => x.IsoAlpha3Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsoName)
            .HasMaxLength(Country.IsoNameMaxLength)
            .IsRequired();

        builder.Property(x => x.RegionCode)
            .HasMaxLength(20);
    }
}
