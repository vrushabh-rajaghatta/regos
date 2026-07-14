using FluentAssertions;

using RegOS.MasterData.Domain.Geography.Country;
using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Domain.Tests.RegulatoryApplication;

public class RegulatoryApplicationLifecycleTests
{
    private static RegulatoryApplicationAggregate NewDraft() =>
        RegulatoryApplicationAggregate.Create(
            new ProductId(Guid.NewGuid()),
            new CountryId(Guid.NewGuid()),
            new AuthorityId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()),
            "Test Application");

    [Fact]
    public void Create_StartsInDraft()
    {
        NewDraft().Status.Should().Be(ApplicationStatus.Draft);
    }

    [Fact]
    public void HappyPath_Draft_Active_OnHold_Active_Closed()
    {
        var application = NewDraft();

        application.Activate();
        application.Status.Should().Be(ApplicationStatus.Active);

        application.PutOnHold();
        application.Status.Should().Be(ApplicationStatus.OnHold);

        application.Activate();
        application.Status.Should().Be(ApplicationStatus.Active);

        application.Close();
        application.Status.Should().Be(ApplicationStatus.Closed);
    }

    [Fact]
    public void Draft_CannotBePutOnHold()
    {
        var act = () => NewDraft().PutOnHold();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.InvalidStatusTransition);
    }

    [Fact]
    public void Draft_CannotBeClosed()
    {
        var act = () => NewDraft().Close();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.InvalidStatusTransition);
    }

    [Fact]
    public void Active_CannotBeActivatedAgain()
    {
        var application = NewDraft();
        application.Activate();

        var act = () => application.Activate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.ApplicationAlreadyActive);
    }

    [Fact]
    public void OnHold_CannotBeClosedDirectly()
    {
        var application = NewDraft();
        application.Activate();
        application.PutOnHold();

        var act = () => application.Close();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.InvalidStatusTransition);
    }

    [Fact]
    public void OnHold_CannotBePutOnHoldAgain()
    {
        var application = NewDraft();
        application.Activate();
        application.PutOnHold();

        var act = () => application.PutOnHold();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.InvalidStatusTransition);
    }

    [Fact]
    public void Closed_IsTerminal()
    {
        var application = NewDraft();
        application.Activate();
        application.Close();

        var activate = () => application.Activate();
        activate.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.ApplicationAlreadyClosed);

        var putOnHold = () => application.PutOnHold();
        putOnHold.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.ApplicationAlreadyClosed);

        var close = () => application.Close();
        close.Should().Throw<InvalidOperationException>()
            .WithMessage(ApplicationErrors.ApplicationAlreadyClosed);
    }
}
