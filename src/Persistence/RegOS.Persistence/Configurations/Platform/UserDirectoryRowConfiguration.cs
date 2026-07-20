using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Persistence.ReadModels;

namespace RegOS.Persistence.Configurations.Platform;

/// <summary>
/// Maps the read-only user directory projection onto the existing Users table.
/// Mapped with ToView so EF treats it as read-only and excludes it from
/// migrations — it owns no schema, it only reads the table the User aggregate
/// owns.
/// </summary>
public sealed class UserDirectoryRowConfiguration
    : IEntityTypeConfiguration<UserDirectoryRow>
{
    public void Configure(EntityTypeBuilder<UserDirectoryRow> builder)
    {
        builder.HasNoKey();

        builder.ToView("Users");

        builder.Property(x => x.Status).HasConversion<int>();
    }
}
