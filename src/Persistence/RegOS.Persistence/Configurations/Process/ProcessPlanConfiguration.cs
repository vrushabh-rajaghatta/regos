using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Process;

public sealed class ProcessPlanConfiguration : IEntityTypeConfiguration<ProcessPlan>
{
    public void Configure(EntityTypeBuilder<ProcessPlan> builder)
    {
        builder.ToTable("ProcessPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProcessPlanId(value))
            .ValueGeneratedNever();

        // Fail-closed, like the objective it serves.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        // Required (ADR-065 decision 3). A plan with no objective is a schedule,
        // and RegOS is not a project-management tool. Cascade: deleting the goal
        // deletes the attempts at it, which is the only reading that leaves no
        // orphaned schedule behind.
        builder.Property(x => x.ProcessObjectiveId)
            .HasConversion(id => id.Value, value => new ProcessObjectiveId(value))
            .IsRequired();

        builder.HasOne<ProcessObjective>()
            .WithMany()
            .HasForeignKey(x => x.ProcessObjectiveId)
            .OnDelete(DeleteBehavior.Cascade);

        // NOT NULL, and ADR-065's versioning model was amended at S003 to say so:
        // instantiation is the only way to create a plan, so every plan has one.
        // Restrict — a version a plan was scheduled from can never be deleted
        // (I4), the same guarantee ADR-035 §2 gives a bound submission.
        builder.Property(x => x.ProcessDefinitionVersionId)
            .HasConversion(
                id => id.Value, value => new ProcessDefinitionVersionId(value))
            .IsRequired();

        builder.HasOne<ProcessDefinitionVersion>()
            .WithMany()
            .HasForeignKey(x => x.ProcessDefinitionVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Name)
            .HasMaxLength(ProcessPlan.NameMaxLength)
            .IsRequired();

        // Half the answer to "why is this milestone on this date?" — the pinned
        // version being the other half.
        builder.Property(x => x.AnchorDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.CurrentStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.ProcessObjectiveId);
        builder.HasIndex(x => new { x.TenantId, x.CurrentStatus });

        builder.Metadata
            .FindNavigation(nameof(ProcessPlan.Steps))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey("ProcessPlanId")
            .OnDelete(DeleteBehavior.Cascade);

        // The ninth append-only history, and the eighth user of the shared
        // mapping (ADR-046 decision 6).
        builder.OwnsStatusHistory<
            ProcessPlan, ProcessPlanStatusEntry, ProcessPlanStatusEntryId>(
            x => x.History,
            "ProcessPlanStatusHistory",
            "ProcessPlanId",
            id => id.Value,
            value => new ProcessPlanStatusEntryId(value),
            ProcessPlanStatusEntry.NoteMaxLength);
    }
}
