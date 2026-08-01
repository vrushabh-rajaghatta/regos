using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.Platform.Contracts;

namespace RegOS.Persistence.Configurations.Platform;

public sealed class UserConfiguration : IEntityTypeConfiguration<UserAggregate>
{
    public void Configure(EntityTypeBuilder<UserAggregate> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        // Nullable strongly-typed reference, like DocumentType.TenantId:
        // null => platform user (ADR-030). Nullability is enforced by the
        // factories, not here — CreateForTenant cannot produce null.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? new TenantId(value.Value)
                    : (TenantId?)null);

        // The Email value object is stored as its normalized string. Email.Create
        // re-runs (idempotent) normalization/validation on read; stored values are
        // always already valid, so it never throws during materialization.
        builder.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        // Defense in depth for the uniqueness policy: an email address
        // identifies exactly one user across RegOS, not one per tenant
        // (ADR-021). Authentication resolves a user before a tenant exists, so
        // the index cannot be tenant-scoped.
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.TenantId);
    }
}
