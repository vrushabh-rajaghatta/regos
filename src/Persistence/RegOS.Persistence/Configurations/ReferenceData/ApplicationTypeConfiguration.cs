using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.ApplicationType;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using ApplicationTypeEntity = RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class ApplicationTypeConfiguration
    : IEntityTypeConfiguration<ApplicationTypeEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationTypeEntity> builder)
    {
        builder.ToTable("ApplicationTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ApplicationTypeId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.AuthorityId)
            .HasConversion(
                id => id.Value,
                value => new AuthorityId(value))
            .IsRequired();

        builder.HasIndex(x => x.AuthorityId);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
