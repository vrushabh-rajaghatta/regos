using RegOS.Platform.Application.Common;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Platform.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileHandler
{
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;
    private readonly ITenantContext _tenantContext;

    public UpdateUserProfileHandler(
        IUserPolicy userPolicy,
        IUserRepository repository,
        ITenantContext tenantContext)
    {
        _userPolicy = userPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, _tenantContext.TenantId, cancellationToken);

        var email = Email.Create(command.Email);

        // Unscoped by tenant (ADR-021), and excluding the user itself so
        // an unchanged email never collides with its own row.
        await _userPolicy.EnsureEmailIsUniqueForUpdateAsync(
            user.Id,
            email,
            cancellationToken);

        // The aggregate owns the invariants (names required, email valid) and
        // the no-op semantics; the handler never reimplements them.
        user.ChangeName(command.FirstName, command.LastName);
        user.ChangeEmail(email);

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
