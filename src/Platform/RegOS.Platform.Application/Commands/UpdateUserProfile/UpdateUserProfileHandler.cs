using RegOS.Platform.Application.Common;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

namespace RegOS.Platform.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileHandler
{
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;

    public UpdateUserProfileHandler(
        IUserPolicy userPolicy,
        IUserRepository repository)
    {
        _userPolicy = userPolicy;
        _repository = repository;
    }

    public async Task HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, command.OrganizationId, cancellationToken);

        var email = Email.Create(command.Email);

        // Scoped to the user's own organization, and excluding the user itself
        // so an unchanged email never collides with its own row.
        await _userPolicy.EnsureEmailIsUniqueForUpdateAsync(
            user.OrganizationId,
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
