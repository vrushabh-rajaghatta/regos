using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Commands.InviteUser;
using RegOS.Platform.Application.Exceptions;
using RegOS.Platform.Application.Services;
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

    private sealed class FakeUserRepository : IUserRepository
    {
        public UserAggregate? Added { get; private set; }

        public Task AddAsync(UserAggregate user, CancellationToken cancellationToken)
        {
            Added = user;
            return Task.CompletedTask;
        }

        public Task<UserAggregate?> GetByIdAsync(UserId id, CancellationToken cancellationToken)
            => Task.FromResult<UserAggregate?>(null);

        public Task UpdateAsync(UserAggregate user, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeUserPolicy : IUserPolicy
    {
        private readonly Exception? _organizationError;
        private readonly Exception? _emailError;

        public FakeUserPolicy(
            Exception? organizationError = null,
            Exception? emailError = null)
        {
            _organizationError = organizationError;
            _emailError = emailError;
        }

        public Task EnsureOrganizationCanAcceptUsersAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken)
            => _organizationError is null
                ? Task.CompletedTask
                : Task.FromException(_organizationError);

        public Task EnsureEmailIsUniqueAsync(
            OrganizationId organizationId,
            Email email,
            CancellationToken cancellationToken)
            => _emailError is null
                ? Task.CompletedTask
                : Task.FromException(_emailError);
    }
}
