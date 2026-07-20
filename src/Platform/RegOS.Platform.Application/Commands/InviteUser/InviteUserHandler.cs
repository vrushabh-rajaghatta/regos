using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Commands.InviteUser;

public sealed class InviteUserHandler
{
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;

    public InviteUserHandler(
        IUserPolicy userPolicy,
        IUserRepository repository)
    {
        _userPolicy = userPolicy;
        _repository = repository;
    }

    public async Task<InviteUserResult> HandleAsync(
        InviteUserCommand command,
        CancellationToken cancellationToken)
    {
        await _userPolicy.EnsureOrganizationCanAcceptUsersAsync(
            command.OrganizationId,
            cancellationToken);

        var email = Email.Create(command.Email);

        await _userPolicy.EnsureEmailIsUniqueAsync(
            command.OrganizationId,
            email,
            cancellationToken);

        var user = UserAggregate.Create(
            command.OrganizationId,
            email,
            command.FirstName,
            command.LastName);

        await _repository.AddAsync(user, cancellationToken);

        return new InviteUserResult(user.Id, user.Status);
    }
}
