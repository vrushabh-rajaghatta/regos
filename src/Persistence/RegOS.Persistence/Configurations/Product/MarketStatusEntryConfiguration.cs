using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;

namespace RegOS.Persistence.Configurations.Product;

/// <summary>
/// Deliberately mirrors <c>RegistrationStatusEntryConfiguration</c>. Where the
/// two histories are identical, keeping their persistence identical too is what
/// will make EPIC-006's extraction mechanical.
/// </summary>
public sealed class MarketStatusEntryConfiguration
    : IEntityTypeConfiguration<MarketStatusEntry>
{
    public void Configure(EntityTypeBuilder<MarketStatusEntry> builder)
    {
        builder.ToTable("MarketStatusHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new MarketStatusEntryId(value));

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // The business date — when this became true in the market.
        builder.Property(x => x.OccurredOn)
            .HasColumnType("date")
            .IsRequired();

        // The system timestamp — when RegOS learned of it. Kept apart from
        // OccurredOn so a late entry stays distinguishable from a backdated one.
        builder.Property(x => x.RecordedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(MarketStatusEntry.NoteMaxLength);

        // Shadow FK to the owning market — the child holds no FK property.
        builder.Property<MedicinalProductId>("MedicinalProductId")
            .HasConversion(
                id => id.Value,
                value => new MedicinalProductId(value))
            .IsRequired();

        builder.HasIndex("MedicinalProductId");

        // Reading a market's history is always chronological, and the derived
        // launch date reads the earliest Launched entry off the same index.
        builder.HasIndex(
            "MedicinalProductId", nameof(MarketStatusEntry.OccurredOn));
    }
}
