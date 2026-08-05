using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Terminology;

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

        // A table rather than a column, because the groupings overlap: Germany
        // is EU and ICH and PIC/S. It replaces `RegionCode`, which was single
        // and could not have said that even if anything had written to it.
        //
        // An owned collection, not reference data: a row has no identity of its
        // own, exists only as part of its country, and is replaced wholesale.
        // The same shape PharmaceuticalProductRoutes uses.
        builder.OwnsMany(x => x.Regions, region =>
        {
            region.ToTable("CountryRegions");

            region.WithOwner().HasForeignKey("CountryId");

            region.Property(x => x.System)
                .HasMaxLength(CodedConcept.SystemMaxLength)
                .IsRequired();

            region.Property(x => x.Code)
                .HasMaxLength(CodedConcept.CodeMaxLength)
                .IsRequired();

            region.Property(x => x.Display)
                .HasMaxLength(CodedConcept.DisplayMaxLength)
                .IsRequired();

            // One statement of each grouping per country, enforced where a race
            // cannot slip past the aggregate's own check.
            region.HasIndex("CountryId", "Code").IsUnique();
        });

        builder.Metadata
            .FindNavigation(nameof(Country.Regions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
