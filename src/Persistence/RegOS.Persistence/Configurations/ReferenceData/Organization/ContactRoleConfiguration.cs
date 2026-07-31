using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.ReferenceData.Organization;

public sealed class ContactRoleConfiguration
    : IEntityTypeConfiguration<ContactRole>
{
    public void Configure(EntityTypeBuilder<ContactRole> builder)
    {
        builder.ToTable("ContactRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ContactRoleId(value));

        // Nullable: null is a role the platform ships, set is a tenant's own.
        // That is what makes the shared-plus-extensible filter possible.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id!.Value,
                value => new TenantId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        // Unique per owner rather than globally: a tenant may coin a code the
        // platform has not, and two tenants may independently coin the same one.
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description).HasMaxLength(500);
    }
}
