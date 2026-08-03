using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.RegulatoryApplication.Application.Commands.CiteStudy;

public sealed class CiteStudyHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly IRegulatoryApplicationRepository _repository;

    public CiteStudyHandler(
        RegOSDbContext dbContext,
        IRegulatoryApplicationRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task HandleAsync(
        CiteStudyCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ClinicalStudyId is not null
            && command.NonClinicalStudyId is not null)
        {
            throw new BusinessRuleViolationException(
                RegulatoryApplicationErrors.CiteOneStudyAtATime);
        }

        var application = await _repository.GetByIdAsync(
            command.ApplicationId, cancellationToken);

        if (application is null)
            throw new NotFoundException(
                RegulatoryApplicationErrors.ApplicationDoesNotExist);

        if (command.ClinicalStudyId is { } clinical)
        {
            // Fail-closed by construction: another tenant's study is invisible
            // to this query, not merely forbidden (ADR-031).
            var exists = await _dbContext.Set<ClinicalStudyAggregate>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == clinical, cancellationToken);

            if (!exists)
                throw new NotFoundException(
                    RegulatoryApplicationErrors.StudyDoesNotExist);

            application.CiteClinicalStudy(clinical);
        }
        else if (command.NonClinicalStudyId is { } nonClinical)
        {
            var exists = await _dbContext.Set<NonClinicalStudyAggregate>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == nonClinical, cancellationToken);

            if (!exists)
                throw new NotFoundException(
                    RegulatoryApplicationErrors.StudyDoesNotExist);

            application.CiteNonClinicalStudy(nonClinical);
        }
        else
        {
            throw new DomainException(RegulatoryApplicationErrors.NoStudyNamed);
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
