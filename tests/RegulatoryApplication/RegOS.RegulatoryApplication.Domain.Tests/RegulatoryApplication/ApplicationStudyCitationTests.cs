using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

using ApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using ApplicationTypeEntity =
    RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.RegulatoryApplication.Domain.Tests;

/// <summary>
/// EPIC-019 S004 — which studies support a filing.
/// </summary>
/// <remarks>
/// A citation is a claim the <em>application</em> makes, which is why it is a
/// child here rather than a join aggregate owned by neither side: nothing about
/// the study changes when a filing cites it (ADR-056).
/// </remarks>
public class ApplicationStudyCitationTests
{
    private static readonly AuthorityId Fda = new(Guid.NewGuid());

    private static ApplicationAggregate NewApplication()
    {
        var type = ApplicationTypeEntity.Create("IND", "Investigational New Drug", Fda);

        return ApplicationAggregate.Create(
            TenantId.New(),
            new GlobalProductId(Guid.NewGuid()),
            new CountryId(Guid.NewGuid()),
            Fda,
            type,
            new OrganizationId(Guid.NewGuid()),
            "Test IND");
    }

    [Fact]
    public void AnApplication_StartsCitingNothing()
    {
        NewApplication().StudyCitations.Should().BeEmpty();
    }

    [Fact]
    public void AnApplication_CitesAStudyOfEitherKind()
    {
        var application = NewApplication();
        var clinical = ClinicalStudyId.New();
        var nonClinical = NonClinicalStudyId.New();

        application.CiteClinicalStudy(clinical);
        application.CiteNonClinicalStudy(nonClinical);

        application.StudyCitations.Should().HaveCount(2);

        application.StudyCitations
            .Select(c => c.StudyId)
            .Should().BeEquivalentTo([clinical.Value, nonClinical.Value]);
    }

    /// <summary>
    /// One claim stated twice is still one claim — and without this, a double
    /// click leaves a row nobody can tell from a real one.
    /// </summary>
    [Fact]
    public void CitingTheSameStudyTwice_ChangesNothing()
    {
        var application = NewApplication();
        var study = NonClinicalStudyId.New();

        application.CiteNonClinicalStudy(study);
        application.CiteNonClinicalStudy(study);

        application.StudyCitations.Should().ContainSingle();
    }

    /// <summary>
    /// The exclusive-or is per citation, not per application: an application
    /// resting on one clinical and one non-clinical study is the ordinary case.
    /// </summary>
    [Fact]
    public void EachCitationNamesOneStudy_OfOneKind()
    {
        var application = NewApplication();

        application.CiteClinicalStudy(ClinicalStudyId.New());
        application.CiteNonClinicalStudy(NonClinicalStudyId.New());

        application.StudyCitations.Should().OnlyContain(
            c => (c.ClinicalStudyId == null) != (c.NonClinicalStudyId == null));
    }

    [Fact]
    public void ACitationCanBeWithdrawn()
    {
        var application = NewApplication();
        var kept = ClinicalStudyId.New();
        var withdrawn = NonClinicalStudyId.New();

        application.CiteClinicalStudy(kept);
        application.CiteNonClinicalStudy(withdrawn);

        application.StopCitingStudy(withdrawn.Value);

        application.StudyCitations.Should().ContainSingle()
            .Which.StudyId.Should().Be(kept.Value);
    }

    [Fact]
    public void WithdrawingACitationThatWasNeverMade_IsRefused()
    {
        var application = NewApplication();

        var withdraw = () => application.StopCitingStudy(Guid.NewGuid());

        withdraw.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*does not cite that study*");
    }

    /// <summary>
    /// The lifecycle governs what may still be <em>filed</em>, not what may
    /// still be <em>recorded</em>. Correcting which studies a closed
    /// application rested on is ordinary regulatory housekeeping.
    /// </summary>
    [Fact]
    public void AClosedApplication_CanStillHaveItsCitationsCorrected()
    {
        var application = NewApplication();

        application.Activate();
        application.Close();

        var study = ClinicalStudyId.New();

        application.CiteClinicalStudy(study);

        application.StudyCitations.Should().ContainSingle();

        application.StopCitingStudy(study.Value);

        application.StudyCitations.Should().BeEmpty();
    }
}
