using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Interaction.Domain.Meetings;
using RegOS.Platform.Contracts;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;

namespace RegOS.Persistence.Configurations.Interaction;

public sealed class HaMeetingConfiguration : IEntityTypeConfiguration<HaMeeting>
{
    public void Configure(EntityTypeBuilder<HaMeeting> builder)
    {
        builder.ToTable("HaMeetings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => HaMeetingId.From(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(id => id.Value, value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(HaMeeting.SubjectMaxLength)
            .IsRequired();

        builder.Property(x => x.AuthorityDivisionId)
            .HasConversion(id => id!.Value.Value, value => new AuthorityDivisionId(value));

        builder.Property(x => x.ScheduledFor);

        builder.Property(x => x.RegulatoryApplicationId)
            .HasConversion(
                id => id!.Value.Value, value => new RegulatoryApplicationId(value));

        builder.Property(x => x.OwnerUserId)
            .HasConversion(id => id!.Value, value => UserId.From(value));

        builder.Property(x => x.Minutes).HasMaxLength(HaMeeting.MinutesMaxLength);
        builder.Property(x => x.Outcome).HasMaxLength(HaMeeting.OutcomeMaxLength);

        builder.Property(x => x.CurrentStatus).HasConversion<int>().IsRequired();

        builder.Ignore(x => x.RaisedOn);
        builder.Ignore(x => x.HeldOn);

        builder.HasIndex(x => new { x.TenantId, x.CurrentStatus, x.ScheduledFor });

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AuthorityDivision>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityDivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsStatusHistory(
            x => x.History,
            "HaMeetingStatusEntries",
            "HaMeetingId",
            (HaMeetingStatusEntryId id) => id.Value,
            value => new HaMeetingStatusEntryId(value),
            HaMeetingStatusEntry.NoteMaxLength);

        // ADR-065 D2 — an annotation the owning aggregate holds, and the fourth
        // context to hold one. SetNull, not Cascade: deleting a plan step must
        // never delete a meeting that happened, and I9 makes the resulting null mean
        // exactly what every other null on this table means — nothing.
        builder.Property(x => x.ProcessStepId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new ProcessStepId(value.Value) : null);

        builder.HasOne<ProcessStep>()
            .WithMany()
            .HasForeignKey(x => x.ProcessStepId)
            .OnDelete(DeleteBehavior.SetNull);

        // "What work did this step involve?" — the read the link exists for.
        builder.HasIndex(x => x.ProcessStepId);
    }
}
