using RegOS.SharedKernel.Abstractions;
using RegOS.Study.Application.Services;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Study.Application.Commands.RegisterNonClinicalStudy;

public sealed class RegisterNonClinicalStudyHandler
{
    private readonly ISponsorStudyIdentifierPolicy _identifierPolicy;
    private readonly INonClinicalStudyRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterNonClinicalStudyHandler(
        ISponsorStudyIdentifierPolicy identifierPolicy,
        INonClinicalStudyRepository repository,
        ITenantContext tenantContext)
    {
        _identifierPolicy = identifierPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<RegisterNonClinicalStudyResult> HandleAsync(
        RegisterNonClinicalStudyCommand command,
        CancellationToken cancellationToken)
    {
        // Same order as the clinical handler, for the same reason: the
        // aggregate produces the canonical identifier, then it is checked.
        var study = NonClinicalStudyAggregate.Register(
            _tenantContext.TenantId,
            command.SponsorStudyIdentifier,
            command.Title);

        await _identifierPolicy.EnsureUnusedAsync(
            _tenantContext.TenantId,
            study.SponsorStudyIdentifier,
            excluding: null,
            cancellationToken);

        await _repository.AddAsync(study, cancellationToken);

        return new RegisterNonClinicalStudyResult(study.Id);
    }
}
