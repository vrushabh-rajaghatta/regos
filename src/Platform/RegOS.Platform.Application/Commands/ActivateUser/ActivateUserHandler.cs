using RegOS.Platform.Application.Common;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.ActivateUser;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. There is no policy here: the
/// lifecycle rule (including idempotency) belongs to the aggregate, and nothing
/// about activation needs to ask the outside world.
/// </summary>
public sealed class ActivateUserHandler
{
    private readonly IUserRepository _repository;

    public ActivateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ActivateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetRequiredAsync(
            command.UserId, command.OrganizationId, cancellationToken);

        user.Activate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
