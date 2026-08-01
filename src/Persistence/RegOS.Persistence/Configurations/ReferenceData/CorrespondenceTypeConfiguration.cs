using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Regulatory.Correspondence;

using CorrespondenceTypeEntity = RegOS.ReferenceData.Domain.Regulatory.Correspondence.CorrespondenceType;

namespace RegOS.Persistence.Configurations.ReferenceData;

public sealed class CorrespondenceTypeConfiguration
    : IEntityTypeConfiguration<CorrespondenceTypeEntity>
{
    public void Configure(EntityTypeBuilder<CorrespondenceTypeEntity> builder)
    {
        builder.ToTable("CorrespondenceTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new CorrespondenceTypeId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(CorrespondenceTypeEntity.CodeMaxLength)
            .IsRequired();

        // Globally unique, not unique-per-authority: unlike SubmissionType,
        // this vocabulary is not authority-scoped.
        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(CorrespondenceTypeEntity.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}
