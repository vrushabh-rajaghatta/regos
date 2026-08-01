using RegOS.Interaction.Application.Services;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Interaction.Application.Commands.RecordCorrespondence;

public sealed class RecordCorrespondenceHandler
{
    private readonly IHaCorrespondencePolicy _policy;
    private readonly IHaCorrespondenceRepository _repository;
    private readonly ITenantContext _tenantContext;

    public RecordCorrespondenceHandler(
        IHaCorrespondencePolicy policy,
        IHaCorrespondenceRepository repository,
        ITenantContext tenantContext)
    {
        _policy = policy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<RecordCorrespondenceResult> HandleAsync(
        RecordCorrespondenceCommand command,
        CancellationToken cancellationToken)
    {
        await _policy.EnsureCanRecordAsync(
            command.AuthorityId,
            command.CorrespondenceTypeId,
            command.RegulatoryApplicationId,
            command.SubmissionId,
            command.RegistrationId,
            cancellationToken);

        var correspondence = HaCorrespondence.Record(
            _tenantContext.TenantId,
            command.AuthorityId,
            command.CorrespondenceTypeId,
            command.Direction,
            command.Subject,
            command.OccurredOn,
            command.ResponseDueOn,
            command.AuthorityReference,
            command.RegulatoryApplicationId,
            command.SubmissionId,
            command.RegistrationId);

        await _repository.AddAsync(correspondence, cancellationToken);

        return new RecordCorrespondenceResult(correspondence.Id);
    }
}
