using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.ValueObjects;
using RegOS.SharedKernel.Abstractions;

using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Commands.InviteUser;

public sealed class InviteUserHandler
{
    private readonly IUserPolicy _userPolicy;
    private readonly IUserRepository _repository;
    private readonly ITenantContext _tenantContext;

    public InviteUserHandler(
        IUserPolicy userPolicy,
        IUserRepository repository,
        ITenantContext tenantContext)
    {
        _userPolicy = userPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<InviteUserResult> HandleAsync(
        InviteUserCommand command,
        CancellationToken cancellationToken)
    {
        // You invite people into your own organization. The caller no longer
        // chooses which one, so inviting into someone else's tenant is not an
        // authorization check that could be forgotten - it is unexpressible.
        var organizationId = new OrganizationId(_tenantContext.TenantId);

        await _userPolicy.EnsureOrganizationCanAcceptUsersAsync(
            organizationId,
            cancellationToken);

        var email = Email.Create(command.Email);

        await _userPolicy.EnsureEmailIsUniqueAsync(
            organizationId,
            email,
            cancellationToken);

        var user = UserAggregate.Create(
            organizationId,
            email,
            command.FirstName,
            command.LastName);

        await _repository.AddAsync(user, cancellationToken);

        return new InviteUserResult(user.Id, user.Status);
    }
}
