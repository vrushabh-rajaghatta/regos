using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using ApplicationTypeEntity =
    RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.RegulatoryApplication.Domain.Tests.RegulatoryApplication;

/// <summary>
/// EPIC-007a S001 — an application is classified, and classified consistently
/// with the authority it is filed with.
/// </summary>
/// <remarks>
/// Both rules are new to the aggregate, and only one of them is new to RegOS.
/// The authority check existed before, in <c>CreateSubmissionHandler</c>, where
/// it ran once per <em>sequence</em> against a value the sequence carried. It
/// now runs once, when the classification is made, on the aggregate that holds
/// both facts.
/// </remarks>
public class RegulatoryApplicationClassificationTests
{
    private static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly AuthorityId Tga =
        new(Guid.Parse("20000000-0000-0000-0000-000000000002"));

    private static ApplicationTypeEntity TypeFor(AuthorityId authorityId) =>
        ApplicationTypeEntity.Create(
            "FDA_IND", "Investigational New Drug Application (IND)", authorityId);

    private static RegulatoryApplicationAggregate Create(
        AuthorityId authorityId,
        ApplicationTypeEntity applicationType) =>
        RegulatoryApplicationAggregate.Create(
            TenantId.New(),
            new GlobalProductId(Guid.NewGuid()),
            new CountryId(Guid.NewGuid()),
            authorityId,
            applicationType,
            new OrganizationId(Guid.NewGuid()),
            "Test Application");

    [Fact]
    public void Create_RecordsTheApplicationType()
    {
        var type = TypeFor(Fda);

        Create(Fda, type).ApplicationTypeId.Should().Be(type.Id);
    }

    [Fact]
    public void Create_WithoutAnApplicationType_IsRejected()
    {
        var act = () => Create(Fda, null!);

        act.Should().Throw<DomainException>()
            .WithMessage(RegulatoryApplicationAggregate.ApplicationTypeRequired);
    }

    [Fact]
    public void Create_WithATypeFromAnotherAuthority_IsRejected()
    {
        // An ARTG inclusion is not something you file with the FDA. Before
        // S001 this was discovered when the first submission was created —
        // by which point the application already existed, misclassified.
        var act = () => Create(Fda, TypeFor(Tga));

        act.Should().Throw<DomainException>()
            .WithMessage(
                RegulatoryApplicationAggregate.ApplicationTypeAuthorityMismatch);
    }

    [Fact]
    public void Create_WithATypeFromItsOwnAuthority_IsAccepted()
    {
        var act = () => Create(Tga, TypeFor(Tga));

        act.Should().NotThrow();
    }
}
