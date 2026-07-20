using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.DeactivateUser;
using RegOS.Platform.Application.Exceptions;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Commands.DeactivateUser;

public sealed class DeactivateUserHandlerTests
{
    private static readonly OrganizationId Organization = OrganizationId.New();

    private static UserAggregate InvitedUser() =>
        UserAggregate.Create(
            Organization,
            Email.Create("john.doe@example.com"),
            "John",
            "Doe");

    [Fact]
    public async Task Deactivates_an_active_user_and_persists_it()
    {
        var user = InvitedUser();
        user.Activate();
        var repository = new FakeUserRepository(user);
        var handler = new DeactivateUserHandler(repository);

        await handler.HandleAsync(
            new DeactivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated.Should().NotBeNull();
        repository.Updated!.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Deactivates_an_invited_user_revoking_the_invitation()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new DeactivateUserHandler(repository);

        await handler.HandleAsync(
            new DeactivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Is_idempotent_so_retries_are_safe()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new DeactivateUserHandler(repository);

        await handler.HandleAsync(
            new DeactivateUserCommand(user.Id), CancellationToken.None);
        await handler.HandleAsync(
            new DeactivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public async Task Preserves_the_profile_because_this_is_not_a_deletion()
    {
        var user = InvitedUser();
        user.Activate();
        var repository = new FakeUserRepository(user);
        var handler = new DeactivateUserHandler(repository);

        await handler.HandleAsync(
            new DeactivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.FirstName.Should().Be("John");
        repository.Updated.LastName.Should().Be("Doe");
        repository.Updated.Email.Value.Should().Be("john.doe@example.com");
        repository.Updated.CreatedOn.Should().Be(user.CreatedOn);
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        var repository = new FakeUserRepository();
        var handler = new DeactivateUserHandler(repository);

        var act = () => handler.HandleAsync(
            new DeactivateUserCommand(UserId.New()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_belongs_to_another_organization()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new DeactivateUserHandler(repository);

        var act = () => handler.HandleAsync(
            new DeactivateUserCommand(user.Id, OrganizationId.New()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }
}
