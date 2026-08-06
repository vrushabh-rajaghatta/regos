using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Process;

public sealed class ProcessDefinitionConfiguration
    : IEntityTypeConfiguration<ProcessDefinition>
{
    public void Configure(EntityTypeBuilder<ProcessDefinition> builder)
    {
        builder.ToTable("ProcessDefinitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ProcessDefinitionId(value))
            .ValueGeneratedNever();

        // Nullable: shared-plus-extensible (ADR-031's second filter shape). A
        // null-tenant playbook is the platform's; a value is a tenant's own.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id!.Value,
                value => new TenantId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(ProcessDefinition.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(ProcessDefinition.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(ProcessDefinition.DescriptionMaxLength);

        builder.Property(x => x.CountryId)
            .HasConversion(id => id.Value, value => new CountryId(value))
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(id => id.Value, value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.ApplicationTypeId)
            .HasConversion(id => id.Value, value => new ApplicationTypeId(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        // One playbook per code per tenant. NULLs are distinct in Postgres, so
        // this does not constrain the platform's own rows against each other —
        // which is why the code is also indexed alone below for the shared set.
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();

        // The scope a plan will be resolved by (S003).
        builder.HasIndex(x => new { x.CountryId, x.AuthorityId, x.ApplicationTypeId });

        builder.Metadata
            .FindNavigation(nameof(ProcessDefinition.Versions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // The shadow FK itself is declared, with its type, in
        // ProcessDefinitionVersionConfiguration — see the note there.
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("ProcessDefinitionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
