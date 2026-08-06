using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Contracts;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Process;

public sealed class ProcessObjectiveConfiguration
    : IEntityTypeConfiguration<ProcessObjective>
{
    public void Configure(EntityTypeBuilder<ProcessObjective> builder)
    {
        builder.ToTable("ProcessObjectives");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProcessObjectiveId(value))
            .ValueGeneratedNever();

        // Fail-closed, unlike ProcessDefinition (ADR-031's first filter shape).
        // A playbook is knowledge RegOS can ship; an objective is a company's own
        // strategy, which nothing shared could ever be.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        // What it is about. A cross-context foreign key, no navigation.
        builder.Property(x => x.GlobalProductId)
            .HasConversion(id => id.Value, value => new GlobalProductId(value))
            .IsRequired();

        builder.HasOne<GlobalProduct>()
            .WithMany()
            .HasForeignKey(x => x.GlobalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CountryId)
            .HasConversion(id => id.Value, value => new CountryId(value))
            .IsRequired();

        // The confirmation seam (ADR-065 D8). Nullable because the market record
        // may not exist yet — not because the objective is incomplete. Restrict,
        // not Cascade: retiring a market record must never delete the strategy
        // that asked for it.
        builder.Property(x => x.MedicinalProductId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new MedicinalProductId(value.Value) : null);

        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // id.Value.Value, not id.Value: RegulatoryApplicationId is one of the 15
        // legacy `record struct` ids ADR-043 is still migrating, so the outer
        // Value unwraps Nullable<T> and the inner one the struct. The ids this
        // context declares are all `sealed class : StronglyTypedId` (ES-020) —
        // this line is the seam between the two forms, not a pattern to copy.
        builder.Property(x => x.RegulatoryApplicationId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new RegulatoryApplicationId(value.Value)
                    : (RegulatoryApplicationId?)null);

        builder.Property(x => x.Name)
            .HasMaxLength(ProcessObjective.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Rationale)
            .HasMaxLength(ProcessObjective.RationaleMaxLength);

        builder.Property(x => x.OwnerUserId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new UserId(value.Value) : null);

        // An intention, not a schedule — a calendar date in a jurisdiction.
        builder.Property(x => x.TargetCompletionOn).HasColumnType("date");

        builder.Property(x => x.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        // "What are we working towards in this market?" — the list read.
        builder.HasIndex(x => new { x.TenantId, x.CurrentStatus });
        builder.HasIndex(x => new { x.GlobalProductId, x.CountryId });

        // The eighth append-only history, and the seventh user of the shared
        // mapping (ADR-046 decision 6). The configuration is shared; the entry
        // type and its rules are not, which is exactly the scope ADR-042 set.
        builder.OwnsStatusHistory<
            ProcessObjective, ProcessObjectiveStatusEntry, ProcessObjectiveStatusEntryId>(
            x => x.History,
            "ProcessObjectiveStatusHistory",
            "ProcessObjectiveId",
            id => id.Value,
            value => new ProcessObjectiveStatusEntryId(value),
            ProcessObjectiveStatusEntry.NoteMaxLength);
    }
}
