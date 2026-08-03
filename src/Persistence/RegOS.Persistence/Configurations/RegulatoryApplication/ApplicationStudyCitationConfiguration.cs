using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

using ClinicalStudyEntity =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyEntity =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Persistence.Configurations.RegulatoryApplication;

public sealed class ApplicationStudyCitationConfiguration
    : IEntityTypeConfiguration<ApplicationStudyCitation>
{
    public void Configure(EntityTypeBuilder<ApplicationStudyCitation> builder)
    {
        builder.ToTable("ApplicationStudyCitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ApplicationStudyCitationId.From(value));

        builder.Property(x => x.ClinicalStudyId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? ClinicalStudyId.From(value.Value)
                    : null);

        builder.Property(x => x.NonClinicalStudyId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null
                    ? NonClinicalStudyId.From(value.Value)
                    : null);

        builder.Property(x => x.CitedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Derived from whichever id is set — not a column, because a third copy
        // of the same guid is a third thing that can disagree.
        builder.Ignore(x => x.StudyId);

        // Shadow FK to the owning application, declared with the aggregate's id
        // and its converter so it is compatible with the principal key — and so
        // the unique indexes below have a property to name. Non-nullable comes
        // for free here: RegulatoryApplicationId is still a record struct, so
        // the optional-shadow-FK trap IdentityConventionTests carries does not
        // apply until it is migrated.
        builder.Property<RegulatoryApplicationId>("ApplicationId")
            .HasConversion(
                id => id.Value,
                value => new RegulatoryApplicationId(value))
            .IsRequired();

        // Restrict: a study a filing rests on must not be deleted out from
        // under it, the same call the placement's references make.
        builder.HasOne<ClinicalStudyEntity>()
            .WithMany()
            .HasForeignKey(x => x.ClinicalStudyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<NonClinicalStudyEntity>()
            .WithMany()
            .HasForeignKey(x => x.NonClinicalStudyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClinicalStudyId);
        builder.HasIndex(x => x.NonClinicalStudyId);

        // Mirrors the aggregate's idempotence: an application cites a study
        // once. Two unique indexes rather than one, because they are two
        // columns naming two aggregates — and each is filtered, since "cites no
        // clinical study" is the ordinary state of a nonclinical citation.
        builder.HasIndex("ApplicationId", nameof(ApplicationStudyCitation.ClinicalStudyId))
            .IsUnique()
            .HasFilter("\"ClinicalStudyId\" IS NOT NULL");

        builder.HasIndex("ApplicationId", nameof(ApplicationStudyCitation.NonClinicalStudyId))
            .IsUnique()
            .HasFilter("\"NonClinicalStudyId\" IS NOT NULL");
    }
}
