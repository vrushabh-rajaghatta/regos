using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Persistence.Configurations.Registration;

public sealed class RegistrationStatusEntryConfiguration
    : IEntityTypeConfiguration<RegistrationStatusEntry>
{
    public void Configure(EntityTypeBuilder<RegistrationStatusEntry> builder)
    {
        builder.ToTable("RegistrationStatusHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RegistrationStatusEntryId(value));

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // The business date — when the decision took effect in the world.
        builder.Property(x => x.OccurredOn)
            .HasColumnType("date")
            .IsRequired();

        // The system timestamp — when RegOS learned of it. Kept apart from
        // OccurredOn so a late entry stays distinguishable from a backdated one.
        builder.Property(x => x.RecordedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(RegistrationStatusEntry.NoteMaxLength);

        // Shadow FK to the owning registration — the child holds no FK property.
        builder.Property<RegistrationId>("RegistrationId")
            .HasConversion(
                id => id.Value,
                value => new RegistrationId(value));

        builder.HasIndex("RegistrationId");

        // Reading a registration's history is always chronological.
        builder.HasIndex("RegistrationId", nameof(RegistrationStatusEntry.OccurredOn));
    }
}
