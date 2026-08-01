using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.GiveCommitment;

public sealed class GiveCommitmentHandler
{
    private readonly ICommitmentRepository _repository;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GiveCommitmentHandler(
        ICommitmentRepository repository,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<GiveCommitmentResult> HandleAsync(
        GiveCommitmentCommand command,
        CancellationToken cancellationToken)
    {
        var authorityExists = await _dbContext.Authorities
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.AuthorityId, cancellationToken);

        if (!authorityExists)
            throw new NotFoundException("The health authority was not found.");

        var commitment = Commitment.Give(
            _tenantContext.TenantId,
            command.AuthorityId,
            command.Title,
            command.GivenOn,
            command.DueOn,
            command.Description,
            command.OwnerUserId,
            command.RegistrationId,
            command.RegulatoryApplicationId,
            command.SourceCorrespondenceId);

        await _repository.AddAsync(commitment, cancellationToken);

        return new GiveCommitmentResult(commitment.Id);
    }
}
