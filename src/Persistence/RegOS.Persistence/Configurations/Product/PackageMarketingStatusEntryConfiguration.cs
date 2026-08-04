using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;

namespace RegOS.Persistence.Configurations.Product;

/// <summary>
/// Deliberately mirrors <c>MarketStatusEntryConfiguration</c>, which mirrors
/// <c>RegistrationStatusEntryConfiguration</c>. Three identical shapes, kept
/// identical on purpose: whenever the extraction is finally made it should be
/// mechanical rather than a reconciliation.
/// </summary>
public sealed class PackageMarketingStatusEntryConfiguration
    : IEntityTypeConfiguration<PackageMarketingStatusEntry>
{
    public void Configure(EntityTypeBuilder<PackageMarketingStatusEntry> builder)
    {
        builder.ToTable("PackageMarketingStatusHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new PackageMarketingStatusEntryId(value));

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // The business date — when this became true for this pack.
        builder.Property(x => x.OccurredOn)
            .HasColumnType("date")
            .IsRequired();

        // The system timestamp — when RegOS learned of it. Kept apart from
        // OccurredOn so a late entry stays distinguishable from a backdated one.
        builder.Property(x => x.RecordedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(PackageMarketingStatusEntry.NoteMaxLength);

        // Shadow FK to the owning pack — the child holds no FK property.
        //
        // **Required, and this is the line that has to be written by hand.**
        // PackagedProductId is a reference type, so a shadow property declared
        // with it is nullable by default, and an optional FK severs instead of
        // deleting: removing a pack would leave its history behind with a null
        // parent rather than removing it. AggregateChildArchitectureTests exists
        // because this was missed twice.
        builder.Property<PackagedProductId>("PackagedProductId")
            .HasConversion(
                id => id.Value,
                value => new PackagedProductId(value))
            .IsRequired();

        builder.HasIndex("PackagedProductId");

        // Reading a pack's history is always chronological.
        builder.HasIndex(
            "PackagedProductId",
            nameof(PackageMarketingStatusEntry.OccurredOn));
    }
}
