using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Submission.Application.Commands.ReportStudyOnPlacement;

/// <summary>
/// Records which study a placement reports.
/// </summary>
/// <remarks>
/// The existence check lives here rather than in the aggregate for the reason
/// <c>PlaceSubmissionDocument</c> gives about template sections: studies are
/// another context, and an aggregate that reached across a context boundary to
/// validate a foreign id would be worse than a handler owning the rule.
/// </remarks>
public sealed class ReportStudyOnPlacementHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ISubmissionRepository _repository;

    public ReportStudyOnPlacementHandler(
        RegOSDbContext dbContext,
        ISubmissionRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task HandleAsync(
        ReportStudyOnPlacementCommand command,
        CancellationToken cancellationToken)
    {
        // Caught here rather than silently resolved: a caller naming two
        // studies has a bug, and picking one for them would file the document
        // under a study nobody chose.
        if (command.ClinicalStudyId is not null
            && command.NonClinicalStudyId is not null)
        {
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.PlacementReportsOneStudy);
        }

        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        if (command.ClinicalStudyId is { } clinical)
        {
            // The tenant query filter makes this fail closed: another tenant's
            // study is not merely forbidden here, it is invisible (ADR-031).
            var exists = await _dbContext.Set<ClinicalStudyAggregate>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == clinical, cancellationToken);

            if (!exists)
                throw new NotFoundException(
                    SubmissionRuleErrors.StudyDoesNotExist);

            submission.ReportClinicalStudy(
                command.SubmissionDocumentId, clinical);
        }
        else if (command.NonClinicalStudyId is { } nonClinical)
        {
            var exists = await _dbContext.Set<NonClinicalStudyAggregate>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == nonClinical, cancellationToken);

            if (!exists)
                throw new NotFoundException(
                    SubmissionRuleErrors.StudyDoesNotExist);

            submission.ReportNonClinicalStudy(
                command.SubmissionDocumentId, nonClinical);
        }
        else
        {
            submission.ClearReportedStudy(command.SubmissionDocumentId);
        }

        await _repository.UpdateAsync(submission, cancellationToken);
    }
}
