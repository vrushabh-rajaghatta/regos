using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;

using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Persistence.Configurations.Platform;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshTokenAggregate>
{
    public void Configure(EntityTypeBuilder<RefreshTokenAggregate> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RefreshTokenId(value));

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        // SHA-256 as uppercase hex is always 64 characters. Fixed length
        // because a varying one would suggest the format is negotiable.
        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.RevokedOn);

        builder.Property(x => x.ReplacedBy)
            .HasConversion(
                id => id!.Value,
                value => new RefreshTokenId(value));

        // Unique: every issued token is distinct, so a collision would mean
        // either the RNG or the hash failed, and the database should refuse it
        // rather than let two sessions share a lookup key.
        builder.HasIndex(x => x.TokenHash).IsUnique();

        // Sign-out and replay handling both sweep a user's tokens.
        builder.HasIndex(x => x.UserId);

        // A refresh token is meaningless without its user, so the schema
        // enforces that lifetime.
        //
        // Note this does NOT satisfy ADR-023 as written: that ADR requires the
        // satellite's primary key to BE the principal's identity, and this key
        // is its own. It meets the other two conditions and the reason behind
        // all three — the lifecycle dependency is absolute — but the rule as
        // stated does not cover it. ADR-023's own "Revisit When" anticipated
        // this: a second candidate is where the pattern gets named properly.
        // Flagged for a decision rather than settled by widening the rule here.
        builder.HasOne<UserAggregate>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
