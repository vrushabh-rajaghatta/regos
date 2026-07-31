using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Persistence.Configurations.ReferenceData.Organization;

public sealed class IdentifierSchemeConfiguration
    : IEntityTypeConfiguration<IdentifierScheme>
{
    public void Configure(EntityTypeBuilder<IdentifierScheme> builder)
    {
        builder.ToTable("IdentifierSchemes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new IdentifierSchemeId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        // A world fact, not a tenant's list: "FEI" means one registry
        // everywhere, so the code is globally unique with no tenant in the key.
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Issuer)
            .HasMaxLength(150)
            .IsRequired();
    }
}
