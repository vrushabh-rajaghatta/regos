using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.SharedKernel.Primitives;

using AuthorityEntity = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using SubmissionTypeEntity = RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

namespace RegOS.Persistence.Configurations.ReferenceData.Blueprint;

public sealed class RegulatoryTemplateConfiguration
    : IEntityTypeConfiguration<RegulatoryTemplate>
{
    public void Configure(EntityTypeBuilder<RegulatoryTemplate> builder)
    {
        builder.ToTable("RegulatoryTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RegulatoryTemplateId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(RegulatoryTemplate.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(RegulatoryTemplate.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(
                id => id.Value,
                value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.SubmissionTypeId)
            .HasConversion(
                id => id.Value,
                value => new SubmissionTypeId(value))
            .IsRequired();

        // Nullable strongly-typed reference: null => platform-shared template.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new TenantId(value.Value)
                    : (TenantId?)null);

        builder.Property(x => x.Source)
            .HasMaxLength(RegulatoryTemplate.SourceMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Shared template codes are globally unique; tenant-scoped uniqueness
        // arrives with tenant-authored templates (a later epic).
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL");

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.AuthorityId);
        builder.HasIndex(x => x.SubmissionTypeId);

        // Reference data is protected from deletion.
        builder.HasOne<AuthorityEntity>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SubmissionTypeEntity>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant extensions reference their owning tenant; shared templates
        // leave this null.
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ownership: RegulatoryTemplate (1) -> Versions (N). The child has no
        // FK property, so EF uses a shadow "RegulatoryTemplateId".
        builder.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey("RegulatoryTemplateId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(RegulatoryTemplate.Versions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
