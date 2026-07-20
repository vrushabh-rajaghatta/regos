using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.ActivateUser;
using RegOS.Platform.Application.Exceptions;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Tests.Commands.ActivateUser;

public sealed class ActivateUserHandlerTests
{
    private static readonly OrganizationId Organization = OrganizationId.New();

    private static UserAggregate InvitedUser() =>
        UserAggregate.Create(
            Organization,
            Email.Create("john.doe@example.com"),
            "John",
            "Doe");

    [Fact]
    public async Task Activates_an_invited_user_and_persists_it()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(repository);

        await handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated.Should().NotBeNull();
        repository.Updated!.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Activates_an_inactive_user()
    {
        var user = InvitedUser();
        user.Deactivate();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(repository);

        await handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Is_idempotent_so_retries_are_safe()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(repository);

        await handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);
        await handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Leaves_the_profile_untouched()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(repository);

        await handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.FirstName.Should().Be("John");
        repository.Updated.LastName.Should().Be("Doe");
        repository.Updated.Email.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        var repository = new FakeUserRepository();
        var handler = new ActivateUserHandler(repository);

        var act = () => handler.HandleAsync(
            new ActivateUserCommand(UserId.New()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_belongs_to_another_organization()
    {
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(repository);

        var act = () => handler.HandleAsync(
            new ActivateUserCommand(user.Id, OrganizationId.New()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }
}
