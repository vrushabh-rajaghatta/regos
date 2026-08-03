using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using ApplicationTypeEntity =
    RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Domain.Tests.RegulatoryApplication;

/// <summary>
/// <b>The property existed for the life of the project and could never be given
/// a value.</b> Private setter, absent from <c>Create</c>, mapped by EF,
/// projected by a query, written by nothing — 59 applications in the development
/// database, all null, and no code path that could change one.
/// </summary>
/// <remarks>
/// <b>A persistent property with no acquisition path is incomplete modelling</b>,
/// which is a stronger statement than "unused field": the field existed and the
/// system still could not know its value. It surfaced only when eCTD generation
/// asked *"where does this fact come from?"*
/// </remarks>
public sealed class RecordApplicationNumberTests
{
    /// <summary>
    /// <b>Stored exactly as assigned.</b> FDA issues six digits; Health Canada,
    /// the EMA and the TGA each issue something else. A format rule on the
    /// aggregate would make one authority's convention every authority's law, so
    /// the shape is checked at the boundary that cares (ADR-055).
    /// </summary>
    [Theory]
    [InlineData("123456")]
    [InlineData("000123")]
    [InlineData("IND 123456")]
    [InlineData("HC6-024-1234567")]
    public void AnyNumberTheAuthorityAssigned_IsRecordedAsGiven(string assigned)
    {
        var application = AnApplication();

        application.RecordApplicationNumber(assigned);

        application.ApplicationNumber.Should().Be(assigned);
    }

    [Fact]
    public void SurroundingSpace_IsNotPartOfTheNumber()
    {
        var application = AnApplication();

        application.RecordApplicationNumber("  123456  ");

        application.ApplicationNumber.Should().Be("123456");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NothingAtAll_IsNotANumber(string blank)
    {
        var record = () => AnApplication().RecordApplicationNumber(blank);

        record.Should().Throw<DomainException>()
            .WithMessage(RegulatoryApplicationAggregate.ApplicationNumberRequired);
    }

    /// <summary>
    /// <b>Correctable here, and frozen elsewhere.</b> RegOS's record of an
    /// external fact can simply be wrong, and refusing a correction would force
    /// someone to delete a regulatory record to fix a typo. What stops a change
    /// after filing is a fact about <em>submissions</em>, which this aggregate
    /// cannot see — so the policy enforces it, the same division S003 drew.
    /// </summary>
    [Fact]
    public void ANumberRecordedInError_CanBeCorrected()
    {
        var application = AnApplication();

        application.RecordApplicationNumber("123456");
        application.RecordApplicationNumber("123465");

        application.ApplicationNumber.Should().Be("123465");
    }

    [Fact]
    public void ANumberLongerThanTheColumn_IsRefused()
    {
        var record = () => AnApplication().RecordApplicationNumber(
            new string('1', RegulatoryApplicationAggregate
                .ApplicationNumberMaxLength + 1));

        record.Should().Throw<DomainException>()
            .WithMessage(RegulatoryApplicationAggregate.ApplicationNumberTooLong);
    }

    /// <summary>
    /// An application exists in RegOS before the authority has answered, which
    /// is the ordinary case rather than an edge one — the number arrives with
    /// the acknowledgement. So it is not part of <c>Create</c>.
    /// </summary>
    [Fact]
    public void ANewApplication_HasNoNumberYet()
    {
        AnApplication().ApplicationNumber.Should().BeNull();
    }

    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static RegulatoryApplicationAggregate AnApplication() =>
        RegulatoryApplicationAggregate.Create(
            TenantId.New(),
            new GlobalProductId(Guid.NewGuid()),
            new CountryId(Guid.NewGuid()),
            Fda,
            ApplicationTypeEntity.Create(
                "FDA_IND", "Investigational New Drug Application (IND)", Fda),
            new OrganizationId(Guid.NewGuid()),
            "Test Application");
}
