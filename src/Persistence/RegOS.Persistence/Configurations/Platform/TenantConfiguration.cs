using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Platform;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value));

        builder.Property(x => x.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(x => x.Name);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // Deliberately no filter and no tenant column of its own: the tenant
        // directory is the one table that is global by definition (ADR-031's
        // tier model). Isolation applies to what tenants own, not to the list
        // of tenants itself.
    }
}
