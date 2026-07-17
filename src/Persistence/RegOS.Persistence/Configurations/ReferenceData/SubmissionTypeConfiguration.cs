using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using SubmissionTypeEntity = RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class SubmissionTypeConfiguration
    : IEntityTypeConfiguration<SubmissionTypeEntity>
{
    public void Configure(EntityTypeBuilder<SubmissionTypeEntity> builder)
    {
        builder.ToTable("SubmissionTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SubmissionTypeId(value));

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
