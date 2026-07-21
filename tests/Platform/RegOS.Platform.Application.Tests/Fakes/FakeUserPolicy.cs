using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>
/// Policy stand-in: each rule either passes or fails with a supplied exception,
/// so handler orchestration can be tested without a database.
/// </summary>
public sealed class FakeUserPolicy : IUserPolicy
{
    private readonly Exception? _organizationError;
    private readonly Exception? _emailError;
    private readonly Exception? _updateEmailError;

    public FakeUserPolicy(
        Exception? organizationError = null,
        Exception? emailError = null,
        Exception? updateEmailError = null)
    {
        _organizationError = organizationError;
        _emailError = emailError;
        _updateEmailError = updateEmailError;
    }

    public Task EnsureOrganizationCanAcceptUsersAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
        => _organizationError is null
            ? Task.CompletedTask
            : Task.FromException(_organizationError);

    public Task EnsureEmailIsUniqueAsync(
        Email email,
        CancellationToken cancellationToken)
        => _emailError is null
            ? Task.CompletedTask
            : Task.FromException(_emailError);

    public Task EnsureEmailIsUniqueForUpdateAsync(
        UserId userId,
        Email email,
        CancellationToken cancellationToken)
        => _updateEmailError is null
            ? Task.CompletedTask
            : Task.FromException(_updateEmailError);
}
