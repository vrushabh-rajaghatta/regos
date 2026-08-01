using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Meetings;
using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.BeginMeeting;

public sealed class BeginMeetingHandler
{
    private readonly IHaMeetingRepository _repository;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public BeginMeetingHandler(
        IHaMeetingRepository repository,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<BeginMeetingResult> HandleAsync(
        BeginMeetingCommand command,
        CancellationToken cancellationToken)
    {
        var authorityExists = await _dbContext.Authorities
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.AuthorityId, cancellationToken);

        if (!authorityExists)
            throw new NotFoundException("The health authority was not found.");

        // The same child-belongs-to-parent rule ADR-040 stated generally: a
        // meeting with the FDA cannot name a Health Canada directorate.
        if (command.AuthorityDivisionId is { } divisionId)
        {
            var belongs = await _dbContext.AuthorityDivisions
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == divisionId && x.AuthorityId == command.AuthorityId,
                    cancellationToken);

            if (!belongs)
                throw new BusinessRuleViolationException(
                    "That division does not belong to the selected health authority.");
        }

        var meeting = HaMeeting.Begin(
            _tenantContext.TenantId,
            command.AuthorityId,
            command.Subject,
            command.InitialStatus,
            command.OccurredOn,
            command.ScheduledFor,
            command.AuthorityDivisionId,
            command.RegulatoryApplicationId);

        await _repository.AddAsync(meeting, cancellationToken);

        return new BeginMeetingResult(meeting.Id);
    }
}
