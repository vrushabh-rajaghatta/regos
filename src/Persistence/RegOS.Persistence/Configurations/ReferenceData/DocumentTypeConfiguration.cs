using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.SharedKernel.Primitives;

using DocumentTypeEntity = RegOS.ReferenceData.Domain.DocumentType.DocumentType;

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
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new TenantId(value.Value)
                    : (TenantId?)null);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        // System-type codes are globally unique. Tenant-scoped code
        // uniqueness is added when tenant extensions arrive (a later sprint).
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL");

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

        builder.HasIndex(x => x.TenantId);

        // Optional relationship: tenant extensions reference their
        // owning tenant. System types leave this null.
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
