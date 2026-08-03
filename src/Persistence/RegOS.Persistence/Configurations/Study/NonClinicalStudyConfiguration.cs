using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.SharedKernel.Primitives;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Persistence.Configurations.Study;

public sealed class NonClinicalStudyConfiguration
    : IEntityTypeConfiguration<NonClinicalStudyAggregate>
{
    public void Configure(EntityTypeBuilder<NonClinicalStudyAggregate> builder)
    {
        builder.ToTable("NonClinicalStudies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value, value => NonClinicalStudyId.From(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(x => x.SponsorStudyIdentifier)
            .HasMaxLength(
                NonClinicalStudyAggregate.SponsorStudyIdentifierMaxLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(NonClinicalStudyAggregate.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedOn).IsRequired();

        // See ClinicalStudyConfiguration: this index is the other half of a
        // rule neither table can express on its own.
        builder.HasIndex(x => new { x.TenantId, x.SponsorStudyIdentifier })
            .IsUnique();
    }
}
