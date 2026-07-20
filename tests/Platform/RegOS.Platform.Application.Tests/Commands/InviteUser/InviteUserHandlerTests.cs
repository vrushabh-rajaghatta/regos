using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.InviteUser;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Exceptions;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Commands.InviteUser;

public sealed class InviteUserHandlerTests
{
    private static InviteUserCommand ValidCommand() =>
        new(OrganizationId.New(), "John", "Doe", "john.doe@example.com");

    [Fact]
    public async Task Invite_Succeeds_ReturnsInvitedStatus()
    {
        var handler = new InviteUserHandler(new FakeUserPolicy(), new FakeUserRepository());

        var result = await handler.HandleAsync(ValidCommand(), CancellationToken.None);

        result.Status.Should().Be(UserStatus.Invited);
        result.Id.Should().NotBeNull();
    }

    [Fact]
    public async Task Invite_Succeeds_PersistsUserViaRepository()
    {
        var repository = new FakeUserRepository();
        var command = ValidCommand();
        var handler = new InviteUserHandler(new FakeUserPolicy(), repository);

        await handler.HandleAsync(command, CancellationToken.None);

        repository.Added.Should().NotBeNull();
        repository.Added!.OrganizationId.Should().Be(command.OrganizationId);
        repository.Added.Email.Value.Should().Be("john.doe@example.com");
        repository.Added.Status.Should().Be(UserStatus.Invited);
    }

    [Fact]
    public async Task Invite_WhenOrganizationCannotAcceptUsers_Throws_AndDoesNotPersist()
    {
        var repository = new FakeUserRepository();
        var policy = new FakeUserPolicy(
            organizationError: new BusinessRuleViolationException(PlatformErrors.OrganizationInactive));
        var handler = new InviteUserHandler(policy, repository);

        var act = () => handler.HandleAsync(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        repository.Added.Should().BeNull();
    }

    [Fact]
    public async Task Invite_WhenEmailNotUnique_Throws_AndDoesNotPersist()
    {
        var repository = new FakeUserRepository();
        var policy = new FakeUserPolicy(
            emailError: new BusinessRuleViolationException(PlatformErrors.EmailAlreadyInUse));
        var handler = new InviteUserHandler(policy, repository);

        var act = () => handler.HandleAsync(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        repository.Added.Should().BeNull();
    }

    [Fact]
    public async Task Invite_WhenEmailMalformed_ThrowsDomainException()
    {
        var handler = new InviteUserHandler(new FakeUserPolicy(), new FakeUserRepository());
        var command = new InviteUserCommand(
            OrganizationId.New(), "John", "Doe", "not-an-email");

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
