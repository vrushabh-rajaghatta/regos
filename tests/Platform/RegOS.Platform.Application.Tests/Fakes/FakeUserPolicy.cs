using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Platform.Application.Tests.Fakes;

/// <summary>
/// Policy stand-in: each rule either passes or fails with a supplied exception,
/// so handler orchestration can be tested without a database.
/// </summary>
public sealed class FakeUserPolicy : IUserPolicy
{
    private readonly Exception? _tenantError;
    private readonly Exception? _emailError;
    private readonly Exception? _updateEmailError;

    public FakeUserPolicy(
        Exception? tenantError = null,
        Exception? emailError = null,
        Exception? updateEmailError = null)
    {
        _tenantError = tenantError;
        _emailError = emailError;
        _updateEmailError = updateEmailError;
    }

    public Task EnsureTenantCanAcceptUsersAsync(
        TenantId tenantId,
        CancellationToken cancellationToken)
        => _tenantError is null
            ? Task.CompletedTask
            : Task.FromException(_tenantError);

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
