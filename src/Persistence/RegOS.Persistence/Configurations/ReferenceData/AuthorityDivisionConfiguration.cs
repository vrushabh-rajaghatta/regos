using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Primitives;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class AuthorityDivisionConfiguration
    : IEntityTypeConfiguration<AuthorityDivision>
{
    public void Configure(EntityTypeBuilder<AuthorityDivision> builder)
    {
        builder.ToTable("AuthorityDivisions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new AuthorityDivisionId(value));

        builder.Property(x => x.AuthorityId)
            .HasConversion(
                id => id.Value,
                value => new AuthorityId(value))
            .IsRequired();

        // Null for a platform division, set for a tenant's own —
        // platform-seeded, tenant-augmentable.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id!.Value,
                value => TenantId.From(value));

        builder.Property(x => x.Name)
            .HasMaxLength(AuthorityDivision.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Two tenants may each add a division the platform did not seed
        // without colliding, and neither can shadow its own twice.
        builder.HasIndex(x => new { x.AuthorityId, x.TenantId, x.Name })
            .IsUnique();

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
