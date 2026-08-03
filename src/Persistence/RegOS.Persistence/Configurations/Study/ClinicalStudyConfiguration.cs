using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.SharedKernel.Primitives;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;

namespace RegOS.Persistence.Configurations.Study;

public sealed class ClinicalStudyConfiguration
    : IEntityTypeConfiguration<ClinicalStudyAggregate>
{
    public void Configure(EntityTypeBuilder<ClinicalStudyAggregate> builder)
    {
        builder.ToTable("ClinicalStudies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => ClinicalStudyId.From(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .IsRequired();

        builder.Property(x => x.SponsorStudyIdentifier)
            .HasMaxLength(ClinicalStudyAggregate.SponsorStudyIdentifierMaxLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(ClinicalStudyAggregate.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedOn).IsRequired();

        // Half the rule. The other half is the nonclinical table's index, and
        // neither can see the other — one identifier names one study across
        // both kinds, and only SponsorStudyIdentifierPolicy states that. These
        // close the race the policy cannot.
        builder.HasIndex(x => new { x.TenantId, x.SponsorStudyIdentifier })
            .IsUnique();
    }
}
