using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.ReferenceData.Domain.DocumentType;

using DocumentTypeEntity = RegOS.ReferenceData.Domain.DocumentType.DocumentType;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class DocumentTypeConfiguration
    : IEntityTypeConfiguration<DocumentTypeEntity>
{
    public void Configure(EntityTypeBuilder<DocumentTypeEntity> builder)
    {
        builder.ToTable("DocumentTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new DocumentTypeId(value));

        // Nullable strongly-typed reference: null => system type.
        builder.Property(x => x.OrganizationId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new OrganizationId(value.Value)
                    : (OrganizationId?)null);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        // System-type codes are globally unique. Organization-scoped code
        // uniqueness is added when org extensions arrive (a later sprint).
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"OrganizationId\" IS NULL");

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId);

        // Optional relationship: organization extensions reference their
        // owning organization. System types leave this null.
        builder.HasOne<OrganizationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
