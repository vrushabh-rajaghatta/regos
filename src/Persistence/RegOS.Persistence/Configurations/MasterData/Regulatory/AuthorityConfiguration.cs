using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.MasterData.Domain.Geography.Country;

namespace RegOS.Persistence.Configurations.MasterData.Regulatory;

public sealed class AuthorityConfiguration
    : IEntityTypeConfiguration<Authority>
{
    public void Configure(EntityTypeBuilder<Authority> builder)
    {
        builder.ToTable("Authorities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new AuthorityId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CountryId)
            .HasConversion(
                id => id.Value,
                value => new CountryId(value))
            .IsRequired();

        builder.HasIndex(x => x.CountryId);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
