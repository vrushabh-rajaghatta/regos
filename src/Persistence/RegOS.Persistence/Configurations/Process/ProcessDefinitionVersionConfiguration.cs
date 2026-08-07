using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Persistence.Configurations.Process;

public sealed class ProcessDefinitionVersionConfiguration
    : IEntityTypeConfiguration<ProcessDefinitionVersion>
{
    public void Configure(EntityTypeBuilder<ProcessDefinitionVersion> builder)
    {
        builder.ToTable("ProcessDefinitionVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ProcessDefinitionVersionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // A calendar fact in a jurisdiction, not an instant.
        builder.Property(x => x.EffectiveFrom).HasColumnType("date");

        builder.Property(x => x.PublishedOnUtc);

        // Shadow FK to the owning playbook, declared and required. EF's inferred
        // shadow key is nullable once the id is a reference type, and an optional
        // FK severs instead of cascading — which would also let the unique index
        // below stop constraining parentless rows, since Postgres treats NULLs as
        // distinct. Enforced by AggregateChildArchitectureTests.
        builder.Property<ProcessDefinitionId>("ProcessDefinitionId")
            .HasConversion(id => id.Value, value => new ProcessDefinitionId(value))
            .IsRequired();

        // One version number per playbook, enforced where a race between two
        // concurrent "start a draft" requests cannot slip past the aggregate's
        // own numbering.
        builder.HasIndex(
                "ProcessDefinitionId",
                nameof(ProcessDefinitionVersion.VersionNumber))
            .IsUnique();

        // "Which version would a new plan instantiate from?" — asked on every
        // read of a playbook, and by resolution in S003.
        builder.HasIndex(
            "ProcessDefinitionId",
            nameof(ProcessDefinitionVersion.Status));

        builder.Metadata
            .FindNavigation(nameof(ProcessDefinitionVersion.Steps))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey("ProcessDefinitionVersionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
