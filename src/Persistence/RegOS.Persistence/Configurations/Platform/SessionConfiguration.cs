using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.User;

using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.Platform.Contracts;

namespace RegOS.Persistence.Configurations.Platform;

public sealed class SessionConfiguration
    : IEntityTypeConfiguration<SessionAggregate>
{
    public void Configure(EntityTypeBuilder<SessionAggregate> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SessionId(value));

        builder.Property(x => x.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .IsRequired();

        // Stored raw and bounded. Long enough for any real User-Agent, short
        // enough that it cannot be used as a scratch pad (ADR-029).
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        // 45 characters holds an IPv6 address with an IPv4 tail.
        builder.Property(x => x.CreatedFromIp).HasMaxLength(45);

        builder.Property(x => x.CreatedOn).IsRequired();
        builder.Property(x => x.LastUsedOn).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevokedOn);

        builder.HasIndex(x => x.UserId);

        // A session for a user who no longer exists is unreachable, so its
        // lifetime belongs to the user (ADR-026).
        builder.HasOne<UserAggregate>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
