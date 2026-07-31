using RegOS.Registration.Application.Services;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Abstractions;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Application.Commands.CreateRegistration;

public sealed class CreateRegistrationHandler
{
    private readonly IRegistrationCreationPolicy _creationPolicy;
    private readonly IRegistrationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateRegistrationHandler(
        IRegistrationCreationPolicy creationPolicy,
        IRegistrationRepository repository,
        ITenantContext tenantContext)
    {
        _creationPolicy = creationPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<CreateRegistrationResult> HandleAsync(
        CreateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        await _creationPolicy.EnsureCanCreateAsync(
            command.GlobalProductId,
            command.CountryId,
            command.AuthorityId,
            command.HolderOrganizationId,
            command.OriginatingApplicationId,
            cancellationToken);

        // The owner is ambient (who is asking); the holder stays an explicit
        // command property (who the authorisation belongs to). The same
        // distinction RegulatoryApplication draws between tenant and applicant.
        var registration = RegistrationAggregate.Create(
            _tenantContext.TenantId,
            command.GlobalProductId,
            command.CountryId,
            command.AuthorityId,
            command.HolderOrganizationId,
            command.OccurredOn,
            command.OriginatingApplicationId,
            command.Note);

        await _repository.AddAsync(registration, cancellationToken);

        return new CreateRegistrationResult(registration.Id);
    }
}
