using FluentAssertions;

using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Study.Domain.Tests;

/// <summary>
/// EPIC-019 S001 — a study is two facts, and whose they are.
/// </summary>
public sealed class StudyRegistrationTests
{
    private static readonly TenantId Tenant =
        TenantId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    [Fact]
    public void AClinicalStudy_CarriesTheSponsorsIdentifierAndItsTitle()
    {
        var study = ClinicalStudyAggregate.Register(
            Tenant, "ABC-101", "A Study Of Something In Humans");

        study.SponsorStudyIdentifier.Should().Be("ABC-101");
        study.Title.Should().Be("A Study Of Something In Humans");
        study.TenantId.Should().Be(Tenant);
        study.Id.Should().NotBeNull();
    }

    [Fact]
    public void ANonClinicalStudy_CarriesTheSame_AndIsADifferentThing()
    {
        var study = NonClinicalStudyAggregate.Register(
            Tenant, "TOX-9", "A 13-Week Toxicity Study In Rats");

        study.SponsorStudyIdentifier.Should().Be("TOX-9");
        study.Title.Should().Be("A 13-Week Toxicity Study In Rats");
    }

    /// <summary>
    /// The point of ADR-056 §2 stated as a compile-and-run fact: the two
    /// aggregates share no base type beyond the kernel's, and their ids are not
    /// interchangeable. A shared <c>StudyId</c> would make this test impossible
    /// to write.
    /// </summary>
    [Fact]
    public void TheTwoKinds_ShareNoParent_AndTheirIdentitiesDoNotMix()
    {
        var shared = Guid.NewGuid();

        var clinical = ClinicalStudyId.From(shared);
        var nonClinical = NonClinicalStudyId.From(shared);

        clinical.Value.Should().Be(nonClinical.Value);
        clinical.Equals(nonClinical).Should().BeFalse(
            "two aggregates means two identity spaces (ADR-056); a "
            + "ClinicalStudyId is not a NonClinicalStudyId even on the same "
            + "guid");

        typeof(ClinicalStudyAggregate).BaseType
            .Should().NotBe(typeof(NonClinicalStudyAggregate).BaseType,
                "each derives from AggregateRoot of its own id — the only "
                + "thing they have in common is the kernel");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AStudyWithoutTheSponsorsIdentifier_IsRefused(string identifier)
    {
        var register = () =>
            ClinicalStudyAggregate.Register(Tenant, identifier, "A Title");

        register.Should().Throw<DomainException>()
            .WithMessage("*identifier the sponsor uses*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AStudyWithoutATitle_IsRefused(string title)
    {
        var register = () =>
            NonClinicalStudyAggregate.Register(Tenant, "TOX-9", title);

        register.Should().Throw<DomainException>()
            .WithMessage("*title is required*");
    }

    [Fact]
    public void BothFacts_AreTrimmed_BecauseTheAuthorityMatchesOnThem()
    {
        // E24: FDA's review tooling recognises one study by its study-id, so
        // " ABC-101 " and "ABC-101" cannot be allowed to become two.
        var study = ClinicalStudyAggregate.Register(
            Tenant, "  ABC-101  ", "  A Title  ");

        study.SponsorStudyIdentifier.Should().Be("ABC-101");
        study.Title.Should().Be("A Title");
    }

    [Fact]
    public void AnIdentifierLongerThanAFilenameCanCarry_IsRefused()
    {
        var register = () => ClinicalStudyAggregate.Register(
            Tenant,
            new string('X', ClinicalStudyAggregate
                .SponsorStudyIdentifierMaxLength + 1),
            "A Title");

        register.Should().Throw<DomainException>()
            .WithMessage("*too long*");
    }

    [Fact]
    public void ATitleLongerThanTheColumn_IsRefused()
    {
        var register = () => NonClinicalStudyAggregate.Register(
            Tenant,
            "TOX-9",
            new string('X', NonClinicalStudyAggregate.TitleMaxLength + 1));

        register.Should().Throw<DomainException>()
            .WithMessage("*too long*");
    }

    /// <summary>
    /// The deliberate absence, asserted so it is a decision rather than a gap:
    /// EPIC-007a settled that an authority's format rule lives at the boundary
    /// that needs it, not in the aggregate. <c>RecordApplicationNumber</c> takes
    /// any string and the generator refuses a non-FDA one by name; S003 will do
    /// the same for an identifier it cannot put in a filename.
    /// </summary>
    [Theory]
    [InlineData("study/101")]
    [InlineData("ABC 101")]
    [InlineData("études-101")]
    public void TheDomain_DoesNotPoliceTheIdentifiersFormat(string identifier)
    {
        var study = ClinicalStudyAggregate.Register(
            Tenant, identifier, "A Title");

        study.SponsorStudyIdentifier.Should().Be(identifier);
    }

    [Fact]
    public void ATitleCanBeCorrected()
    {
        var study = NonClinicalStudyAggregate.Register(
            Tenant, "TOX-9", "13-Wek Toxicity Study");

        study.Retitle("13-Week Toxicity Study In Rats");

        study.Title.Should().Be("13-Week Toxicity Study In Rats");
    }

    [Fact]
    public void ACorrectionIsStillATitle()
    {
        var study = ClinicalStudyAggregate.Register(
            Tenant, "ABC-101", "A Title");

        var retitle = () => study.Retitle("  ");

        retitle.Should().Throw<DomainException>();
        study.Title.Should().Be("A Title");
    }

    /// <summary>
    /// A study has no status, and that is a decision (ES-018 is about deletion,
    /// and nothing deletes a study). Asserted so that adding one is a choice
    /// someone makes rather than a column that appears.
    /// </summary>
    [Fact]
    public void AStudy_HasNoLifecycle_BecauseNothingRetiresOne()
    {
        string[] theTwoFactsAndTheirScaffolding =
            ["Id", "TenantId", "SponsorStudyIdentifier", "Title", "CreatedOn"];

        const string reason =
            "ADR-056 §3 admits an attribute only when an external regulatory "
            + "workflow or a demonstrated capability asks for it — this list "
            + "changing is that decision being made";

        typeof(ClinicalStudyAggregate).GetProperties()
            .Select(p => p.Name)
            .Should().BeEquivalentTo(theTwoFactsAndTheirScaffolding, reason);

        typeof(NonClinicalStudyAggregate).GetProperties()
            .Select(p => p.Name)
            .Should().BeEquivalentTo(theTwoFactsAndTheirScaffolding, reason);
    }
}
