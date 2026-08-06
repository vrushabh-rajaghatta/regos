using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Interaction.Domain.Inspections;
using RegOS.Interaction.Domain.Meetings;
using RegOS.Platform.Contracts;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;

namespace RegOS.Persistence.Configurations.Interaction;

public sealed class CommitmentConfiguration : IEntityTypeConfiguration<Commitment>
{
    public void Configure(EntityTypeBuilder<Commitment> builder)
    {
        builder.ToTable("Commitments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => CommitmentId.From(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(id => id.Value, value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(Commitment.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(Commitment.DescriptionMaxLength);

        builder.Property(x => x.DueOn).IsRequired();

        // Held, never navigated to (ES-014, ADR-041). No foreign key: Platform
        // owns users, and a regulatory table must not constrain that lifecycle.
        builder.Property(x => x.OwnerUserId)
            .HasConversion(id => id!.Value, value => UserId.From(value));

        builder.Property(x => x.RegistrationId)
            .HasConversion(id => id!.Value.Value, value => new RegistrationId(value));

        builder.Property(x => x.RegulatoryApplicationId)
            .HasConversion(
                id => id!.Value.Value, value => new RegulatoryApplicationId(value));

        builder.Property(x => x.SourceCorrespondenceId)
            .HasConversion(id => id!.Value, value => HaCorrespondenceId.From(value));

        builder.Property(x => x.SourceMeetingId)
            .HasConversion(id => id!.Value, value => HaMeetingId.From(value));

        builder.Property(x => x.SourceInspectionId)
            .HasConversion(id => id!.Value, value => InspectionId.From(value));

        builder.Property(x => x.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Ignore(x => x.GivenOn);
        builder.Ignore(x => x.FulfilledOn);

        // The due view's hot path: open work, soonest first.
        builder.HasIndex(x => new { x.TenantId, x.CurrentStatus, x.DueOn });
        builder.HasIndex(x => new { x.TenantId, x.OwnerUserId });

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsStatusHistory(
            x => x.History,
            "CommitmentStatusEntries",
            "CommitmentId",
            (CommitmentStatusEntryId id) => id.Value,
            value => new CommitmentStatusEntryId(value),
            CommitmentStatusEntry.NoteMaxLength);

        // ADR-065 D2 — an annotation the owning aggregate holds, and the fourth
        // context to hold one. SetNull, not Cascade: deleting a plan step must
        // never delete a commitment we made to an authority, and I9 makes the resulting null mean
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
