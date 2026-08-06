using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Persistence.Configurations.Process;

public sealed class ProcessStepDefinitionConfiguration
    : IEntityTypeConfiguration<ProcessStepDefinition>
{
    public void Configure(EntityTypeBuilder<ProcessStepDefinition> builder)
    {
        builder.ToTable("ProcessStepDefinitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ProcessStepDefinitionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasMaxLength(ProcessStepDefinition.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(ProcessStepDefinition.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(ProcessStepDefinition.DescriptionMaxLength);

        // Structural grouping into phases. Self-referencing and nullable — a
        // top-level step has no parent. No FK constraint: both rows live in the
        // same version and the aggregate already refuses a parent from another
        // one, so a database-level cycle guard would add nothing the domain does
        // not already enforce at publish.
        builder.Property(x => x.ParentStepId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new ProcessStepDefinitionId(value.Value)
                    : null);

        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.OffsetDays).IsRequired();
        builder.Property(x => x.DurationDays).IsRequired();

        // Shadow FK to the owning version, declared and required — see the note
        // in ProcessDefinitionVersionConfiguration.
        builder.Property<ProcessDefinitionVersionId>("ProcessDefinitionVersionId")
            .HasConversion(
                id => id.Value,
                value => new ProcessDefinitionVersionId(value))
            .IsRequired();

        // One step code per version. The aggregate enforces it too; this is the
        // half a concurrent request cannot slip past.
        builder.HasIndex(
                "ProcessDefinitionVersionId",
                nameof(ProcessStepDefinition.Code))
            .IsUnique();

        builder.HasIndex(x => x.ParentStepId);

        builder.Metadata
            .FindNavigation(nameof(ProcessStepDefinition.Predecessors))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Owned rather than a related entity: a predecessor edge has no identity
        // of its own and nothing outside the step ever references one.
        builder.OwnsMany(x => x.Predecessors, predecessor =>
        {
            predecessor.ToTable("ProcessStepPredecessors");

            predecessor.WithOwner().HasForeignKey("ProcessStepDefinitionId");

            predecessor.Property(x => x.PredecessorStepId)
                .HasConversion(
                    id => id.Value,
                    value => new ProcessStepDefinitionId(value))
                .IsRequired();

            // A step waits for another step at most once. The aggregate refuses
            // a duplicate; this is the concurrent half.
            predecessor.HasIndex(
                    "ProcessStepDefinitionId",
                    nameof(ProcessStepPredecessor.PredecessorStepId))
                .IsUnique();
        });
    }
}
