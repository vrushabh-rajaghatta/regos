using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Persistence.Configurations.Process;

public sealed class ProcessStepConfiguration : IEntityTypeConfiguration<ProcessStep>
{
    public void Configure(EntityTypeBuilder<ProcessStep> builder)
    {
        builder.ToTable("ProcessSteps");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProcessStepId(value))
            .ValueGeneratedNever();

        // Provenance. No FK: the definition version is frozen and Restrict-ed by
        // the plan already, and a second constraint on the same guarantee would
        // add nothing but a second thing to keep true.
        builder.Property(x => x.StepDefinitionId)
            .HasConversion(
                id => id.Value, value => new ProcessStepDefinitionId(value))
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(ProcessStepDefinition.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(ProcessStepDefinition.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(ProcessStepDefinition.DescriptionMaxLength);

        // Self-referencing within the plan, translated at instantiation.
        builder.Property(x => x.ParentStepId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new ProcessStepId(value.Value) : null);

        builder.Property(x => x.Order).IsRequired();

        builder.Property(x => x.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        // Inclusive calendar dates in a jurisdiction, not instants.
        builder.Property(x => x.PlannedStartOn).HasColumnType("date").IsRequired();
        builder.Property(x => x.PlannedEndOn).HasColumnType("date").IsRequired();

        // Shadow FK to the owning plan, declared and required — EF's inferred one
        // is nullable once the id is a reference type, and an optional FK severs
        // instead of cascading (AggregateChildArchitectureTests).
        builder.Property<ProcessPlanId>("ProcessPlanId")
            .HasConversion(id => id.Value, value => new ProcessPlanId(value))
            .IsRequired();

        // One step per code per plan, mirroring the definition's own rule.
        builder.HasIndex("ProcessPlanId", nameof(ProcessStep.Code)).IsUnique();

        // "What is next?" — the read S004 and S005 are built on.
        builder.HasIndex("ProcessPlanId", nameof(ProcessStep.PlannedStartOn));

        // "What can I work on today?" — the read S004 exists for, filtering
        // unsettled steps across a tenant's active plans.
        builder.HasIndex(x => new { x.CurrentStatus, x.PlannedStartOn });

        // The tenth append-only history (ADR-065 I6), and the ninth user of the
        // shared mapping. A step is a child entity rather than a root, and the
        // mapping's root overload still applies because it has its own
        // configuration.
        builder.OwnsStatusHistory<
            ProcessStep, ProcessStepStatusEntry, ProcessStepStatusEntryId>(
            x => x.History,
            "ProcessStepStatusHistory",
            "ProcessStepId",
            id => id.Value,
            value => new ProcessStepStatusEntryId(value),
            ProcessStepStatusEntry.NoteMaxLength);

        builder.Metadata
            .FindNavigation(nameof(ProcessStep.Predecessors))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Predecessors, dependency =>
        {
            dependency.ToTable("ProcessStepDependencies");

            dependency.WithOwner().HasForeignKey("ProcessStepId");

            dependency.Property(x => x.PredecessorStepId)
                .HasConversion(id => id.Value, value => new ProcessStepId(value))
                .IsRequired();

            dependency.HasIndex(
                    "ProcessStepId",
                    nameof(ProcessStepDependency.PredecessorStepId))
                .IsUnique();
        });
    }
}
