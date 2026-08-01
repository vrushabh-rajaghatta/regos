using FluentAssertions;

using RegOS.SharedKernel.Primitives;
using RegOS.Platform.Application.Commands.ActivateUser;
using RegOS.Platform.Application.Tests.Fakes;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;
using RegOS.SharedKernel.Exceptions;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Tests.Commands.ActivateUser;

public sealed class ActivateUserHandlerTests
{
    private static readonly TenantId Organization = TenantId.New();

    private static UserAggregate InvitedUser() =>
        UserAggregate.CreateForTenant(
            Organization,
            Email.Create("john.doe@example.com"),
            "John",
            "Doe");

    /// <summary>
    /// The only state this handler now accepts. Reaching it goes through
    /// acceptance and then deactivation, which is the real sequence.
    /// </summary>
    private static UserAggregate DeactivatedUser()
    {
        var user = InvitedUser();

        user.Activate();
        user.Deactivate();

        return user;
    }

    [Fact]
    public async Task Refuses_to_activate_an_invited_user()
    {
        // The path that produced an Active user with no credential. Closing it
        // is what makes "every Active user has exactly one credential"
        // enforceable rather than aspirational (ADR-027). An invited user
        // becomes active by accepting their invitation.
        var user = InvitedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(
            repository, new FakeTenantContext(Organization));

        var act = () => handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(UserErrors.OnlyInactiveUsersCanBeActivated);

        repository.Updated.Should().BeNull();
        user.Status.Should().Be(UserStatus.Invited);
    }

    [Fact]
    public async Task Refuses_to_activate_a_user_who_is_already_active()
    {
        // Previously idempotent. It is now a state conflict, because "activate"
        // means "reinstate" and there is nothing to reinstate (ADR-009).
        var user = InvitedUser();
        user.Activate();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(
            repository, new FakeTenantContext(Organization));

        var act = () => handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Reinstates_a_deactivated_user()
    {
        var user = DeactivatedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(
            repository, new FakeTenantContext(Organization));

        await handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        repository.Updated!.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Leaves_the_profile_untouched()
    {
        var user = DeactivatedUser();
        var repository = new FakeUserRepository(user);
        var handler = new ActivateUserHandler(
            repository, new FakeTenantContext(Organization));

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
        var handler = new ActivateUserHandler(
            repository, new FakeTenantContext(Organization));

        var act = () => handler.HandleAsync(
            new ActivateUserCommand(UserId.New()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_belongs_to_another_organization()
    {
        var user = DeactivatedUser();
        var repository = new FakeUserRepository(user);
        // The caller's tenant is a different organization, so the user must be
        // invisible. The command cannot express a tenant at all any more.
        var handler = new ActivateUserHandler(
            repository, new FakeTenantContext(TenantId.New()));

        var act = () => handler.HandleAsync(
            new ActivateUserCommand(user.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Updated.Should().BeNull();
    }
}
