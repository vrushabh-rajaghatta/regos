using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Platform.Domain.Aggregates.User;

using UserCredentialAggregate =
    RegOS.Platform.Domain.Aggregates.UserCredential.UserCredential;

namespace RegOS.Persistence.Configurations.Platform;

public sealed class UserCredentialConfiguration
    : IEntityTypeConfiguration<UserCredentialAggregate>
{
    public void Configure(EntityTypeBuilder<UserCredentialAggregate> builder)
    {
        builder.ToTable("UserCredentials");

        // The key is the UserId, which is what enforces at most one credential
        // per user in the schema rather than in a rule someone has to remember.
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("UserId")
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        // Opaque to the domain and to the database alike. The length allows
        // room for the framework's versioned format to grow; nothing parses it.
        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        builder.Property(x => x.UpdatedOn)
            .IsRequired();
    }
}
