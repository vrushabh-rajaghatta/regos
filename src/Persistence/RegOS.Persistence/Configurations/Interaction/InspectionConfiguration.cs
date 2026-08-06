using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Interaction.Domain.Inspections;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Platform.Contracts;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Primitives;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;

namespace RegOS.Persistence.Configurations.Interaction;

public sealed class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => InspectionId.From(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(id => id.Value, value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(Inspection.TitleMaxLength)
            .IsRequired();

        // The thing inspected. A real foreign key: unlike a user, the site is
        // a regulatory record this context may legitimately constrain.
        builder.Property(x => x.OrganizationSiteId)
            .HasConversion(id => id!.Value, value => OrganizationSiteId.From(value));

        builder.Property(x => x.ScheduledFor);

        builder.Property(x => x.OwnerUserId)
            .HasConversion(id => id!.Value, value => UserId.From(value));

        builder.Property(x => x.Outcome).HasMaxLength(Inspection.OutcomeMaxLength);

        builder.Property(x => x.CurrentStatus).HasConversion<int>().IsRequired();

        builder.Ignore(x => x.RaisedOn);
        builder.Ignore(x => x.CompletedOn);

        builder.HasIndex(x => new { x.TenantId, x.CurrentStatus, x.ScheduledFor });
        builder.HasIndex(x => x.OrganizationSiteId);

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<OrganizationSite>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsStatusHistory(
            x => x.History,
            "InspectionStatusEntries",
            "InspectionId",
            (InspectionStatusEntryId id) => id.Value,
            value => new InspectionStatusEntryId(value),
            InspectionStatusEntry.NoteMaxLength);

        // ADR-065 D2 — an annotation the owning aggregate holds, and the fourth
        // context to hold one. SetNull, not Cascade: deleting a plan step must
        // never delete an inspection, and I9 makes the resulting null mean
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
