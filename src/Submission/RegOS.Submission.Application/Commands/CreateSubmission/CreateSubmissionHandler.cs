using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Exceptions;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Commands.CreateSubmission;

public sealed class CreateSubmissionHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ISubmissionRepository _repository;

    public CreateSubmissionHandler(
        RegOSDbContext dbContext,
        ISubmissionRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task<CreateSubmissionResult> HandleAsync(
        CreateSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        // Rule 1 — Application must exist. It is the addressed resource,
        // so its absence is a 404 (see ApplicationNotFoundException).
        var application = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.ApplicationId,
                cancellationToken);

        if (application is null)
            throw new ApplicationNotFoundException(
                SubmissionRuleErrors.ApplicationDoesNotExist);

        // Rule 2 — Submission Type must exist.
        var submissionType = await _dbContext.SubmissionTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.SubmissionTypeId,
                cancellationToken);

        if (submissionType is null)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.SubmissionTypeDoesNotExist);

        // Rule 3 — Submission Type must belong to the same Authority as the
        // Application. First link between the execution hierarchy and the
        // reference-data hierarchy.
        if (submissionType.AuthorityId != application.AuthorityId)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.SubmissionTypeAuthorityMismatch);

        // Rule 4 — A closed Application accepts no new Submissions.
        if (application.Status == ApplicationStatus.Closed)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.ApplicationClosed);

        var submission = SubmissionAggregate.Create(
            command.ApplicationId,
            command.SubmissionTypeId,
            command.Name);

        await _repository.AddAsync(submission, cancellationToken);

        return new CreateSubmissionResult(submission.Id);
    }
}
