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

        // The eCTD wire token. Nullable, and the null is load-bearing: it means
        // this authority's vocabulary has not been modelled, which is true of
        // every authority but FDA.
        builder.Property(x => x.Token)
            .HasMaxLength(50);

        // Two authorities may well both use "fdast1"-shaped tokens of their own,
        // so uniqueness is per authority rather than global — and only over rows
        // that have one, since "not modelled yet" is not a value that can clash.
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
