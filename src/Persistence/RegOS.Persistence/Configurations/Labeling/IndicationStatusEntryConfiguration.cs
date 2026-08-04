using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// One regulatory decision, and the day it took effect.
/// </summary>
/// <remarks>
/// Append-only in the domain, and nothing here offers an update path. A
/// regulatory decision should not disappear: an indication must not silently
/// become withdrawn, it must have become withdrawn on a date.
/// </remarks>
public sealed class IndicationStatusEntryConfiguration
    : IEntityTypeConfiguration<IndicationStatusEntry>
{
    public void Configure(EntityTypeBuilder<IndicationStatusEntry> builder)
    {
        builder.ToTable("IndicationStatusHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new IndicationStatusEntryId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // A calendar fact in a jurisdiction, not an instant.
        builder.Property(x => x.OccurredOn)
            .HasColumnType("date")
            .IsRequired();

        // When RegOS learned of it — a different date, and both get asked about.
        builder.Property(x => x.RecordedOnUtc)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(IndicationStatusEntry.NoteMaxLength);

        builder.Property<IndicationId>("IndicationId")
            .HasConversion(id => id.Value, value => new IndicationId(value))
            .IsRequired();

        builder.HasIndex("IndicationId", nameof(IndicationStatusEntry.OccurredOn));
    }
}
