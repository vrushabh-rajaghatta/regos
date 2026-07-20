using RegOS.Platform.Application.Common;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.DeactivateUser;

/// <summary>
/// The mirror of activation: load, invoke the aggregate behaviour, persist.
/// The lifecycle rule and its idempotency belong to the aggregate.
/// </summary>
public sealed class DeactivateUserHandler
{
    private readonly IUserRepository _repository;

    public DeactivateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeactivateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, command.OrganizationId, cancellationToken);

        user.Deactivate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
