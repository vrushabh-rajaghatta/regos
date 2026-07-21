using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.PasswordReset;
using RegOS.Platform.Domain.Aggregates.User;

using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Persistence.Configurations.Platform;

public sealed class PasswordResetConfiguration
    : IEntityTypeConfiguration<PasswordResetAggregate>
{
    public void Configure(EntityTypeBuilder<PasswordResetAggregate> builder)
    {
        builder.ToTable("PasswordResets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new PasswordResetId(value));

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        // SHA-256 as uppercase hex is always 64 characters.
        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.ConsumedOn);
        builder.Property(x => x.RevokedOn);

        // Every issued token is distinct, so a collision would mean the RNG or
        // the hash failed and the database should refuse it.
        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasIndex(x => x.UserId);

        // A reset for a user who no longer exists could not change anybody's
        // password, so its lifetime belongs to the user (ADR-026). One-to-many:
        // a user may have asked, and abandoned, many resets.
        builder.HasOne<UserAggregate>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
