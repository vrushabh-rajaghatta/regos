using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegOS.SharedKernel.Exceptions;

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
        // so its absence is a 404 (see NotFoundException).
        var application = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.ApplicationId,
                cancellationToken);

        if (application is null)
            throw new NotFoundException(
                SubmissionRuleErrors.ApplicationDoesNotExist);

        // Rule 2 — A closed Application accepts no new Submissions.
        if (application.Status == ApplicationStatus.Closed)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.ApplicationClosed);

        // The application type is no longer supplied here, and the two rules
        // that used to guard it are gone with it (EPIC-007a S001). It exists,
        // and it belongs to this application's authority, because
        // RegulatoryApplication.Create refused to produce an application for
        // which either was false — checked once at classification rather than
        // re-checked on every sequence.

        // Resolve the blueprint that governs this submission. Deliberately not
        // a rule: an application type with no published template produces an
        // unbound submission rather than a failure (incomplete reference data
        // must never block the business).
        var boundTemplateVersionId = await ResolveTemplateVersionAsync(
            application.ApplicationTypeId, cancellationToken);

        // The tenant comes from the parent application, not from the ambient
        // context: a submission structurally cannot carry a different tenant
        // than the application it belongs to (ADR-031).
        var submission = SubmissionAggregate.Create(
            application.TenantId,
            command.ApplicationId,
            command.Title,
            command.Format,
            boundTemplateVersionId);

        await _repository.AddAsync(submission, cancellationToken);

        return new CreateSubmissionResult(submission.Id);
    }

    /// <summary>
    /// Finds the published template version that governs an application type, or
    /// null when none does. The submission is pinned to that version so a later
    /// publication never changes what an in-flight submission must contain.
    /// </summary>
    private async Task<RegulatoryTemplateVersionId?> ResolveTemplateVersionAsync(
        ApplicationTypeId applicationTypeId,
        CancellationToken cancellationToken)
    {
        // Small, read-mostly reference data: materialize the candidates (the
        // tenant filter already limits these to shared + own templates) and
        // choose in memory, rather than fighting LINQ translation over
        // strongly-typed ids and enums.
        var candidates = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
            .Where(t => t.ApplicationTypeId == applicationTypeId
                && t.Status == RegulatoryTemplateStatus.Active)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var version = candidates
            // A tenant's own template shadows the platform-shared one, so the
            // choice of *template* is made first — picking the newest version
            // across all candidates would let a shared template outrank the
            // tenant's own.
            .OrderByDescending(t => t.TenantId != null)
            .Select(t => t.Versions
                // Within a template: published, effective today, newest wins.
                .Where(v => v.Status == TemplateVersionStatus.Published
                    && (v.EffectiveFrom is null || v.EffectiveFrom <= today)
                    && (v.EffectiveTo is null || v.EffectiveTo >= today))
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefault())
            .FirstOrDefault(v => v is not null);

        return version?.Id;
    }
}
