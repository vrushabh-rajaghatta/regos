using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionSubType;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using SubmissionSubTypeEntity =
    RegOS.ReferenceData.Domain.SubmissionSubType.SubmissionSubType;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class SubmissionSubTypeConfiguration
    : IEntityTypeConfiguration<SubmissionSubTypeEntity>
{
    public void Configure(EntityTypeBuilder<SubmissionSubTypeEntity> builder)
    {
        builder.ToTable("SubmissionSubTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SubmissionSubTypeId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.AuthorityId, x.Token })
            .IsUnique()
            .HasFilter("\"Token\" IS NOT NULL");

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
