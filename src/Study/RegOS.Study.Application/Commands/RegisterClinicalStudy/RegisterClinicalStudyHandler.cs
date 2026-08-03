using RegOS.SharedKernel.Abstractions;
using RegOS.Study.Application.Services;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;

namespace RegOS.Study.Application.Commands.RegisterClinicalStudy;

public sealed class RegisterClinicalStudyHandler
{
    private readonly ISponsorStudyIdentifierPolicy _identifierPolicy;
    private readonly IClinicalStudyRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RegisterClinicalStudyHandler(
        ISponsorStudyIdentifierPolicy identifierPolicy,
        IClinicalStudyRepository repository,
        ITenantContext tenantContext)
    {
        _identifierPolicy = identifierPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<RegisterClinicalStudyResult> HandleAsync(
        RegisterClinicalStudyCommand command,
        CancellationToken cancellationToken)
    {
        // Built first, so the uniqueness check reads the canonical trimmed
        // identifier rather than whatever arrived on the wire — " ABC-1 " and
        // "ABC-1" are the same study to FDA and must be here too. It also puts
        // the shape errors (400) ahead of the conflict (409), which is the
        // order a user can act on.
        var study = ClinicalStudyAggregate.Register(
            _tenantContext.TenantId,
            command.SponsorStudyIdentifier,
            command.Title);

        await _identifierPolicy.EnsureUnusedAsync(
            _tenantContext.TenantId,
            study.SponsorStudyIdentifier,
            excluding: null,
            cancellationToken);

        await _repository.AddAsync(study, cancellationToken);

        return new RegisterClinicalStudyResult(study.Id);
    }
}
