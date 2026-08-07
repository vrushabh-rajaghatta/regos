using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
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

        // Rules 3-7 — which regulatory activity this sequence belongs to.
        var classification = await ClassifyAsync(
            command, application, cancellationToken);

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
            classification,
            boundTemplateVersionId);

        await _repository.AddAsync(submission, cancellationToken);

        return new CreateSubmissionResult(submission.Id);
    }

    /// <summary>
    /// Turns what arrived over the wire into the one of two shapes the domain
    /// accepts, refusing everything else here rather than letting a
    /// contradiction reach the aggregate.
    /// </summary>
    private async Task<SubmissionClassification> ClassifyAsync(
        CreateSubmissionCommand command,
        RegulatoryApplicationAggregate application,
        CancellationToken cancellationToken)
    {
        // Rule 3 — exactly one of "starts" or "continues". Both, or neither, is
        // not an incomplete request but a meaningless one.
        var opens = command.SubmissionTypeId is not null;
        var continues = command.OriginatingSubmissionId is not null;

        if (opens == continues)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.ActivityChoiceNotExclusive);

        // Rule 4 — the sequence action exists, and belongs to the authority this
        // application is filed with.
        var subType = await _dbContext.SubmissionSubTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.SubmissionSubTypeId,
                cancellationToken);

        if (subType is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionSubTypeDoesNotExist);

        if (subType.AuthorityId != application.AuthorityId)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.SubmissionSubTypeNotForThisAuthority);

        if (command.SubmissionTypeId is { } submissionTypeId)
        {
            // Rule 5 — same two checks for the activity itself.
            var type = await _dbContext.SubmissionTypes
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == submissionTypeId, cancellationToken);

            if (type is null)
                throw new NotFoundException(
                    SubmissionRuleErrors.SubmissionTypeDoesNotExist);

            if (type.AuthorityId != application.AuthorityId)
                throw new BusinessRuleViolationException(
                    SubmissionRuleErrors.SubmissionTypeNotForThisAuthority);

            return SubmissionClassification.Opens(
                submissionTypeId, command.SubmissionSubTypeId);
        }

        // Rule 6 — the origin exists. The tenant query filter is doing real work
        // here: a submission in another tenant is not found rather than
        // forbidden (ADR-031).
        var origin = await _dbContext.Submissions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.OriginatingSubmissionId!,
                cancellationToken);

        if (origin is null)
            throw new NotFoundException(
                SubmissionRuleErrors.OriginatingSubmissionDoesNotExist);

        // Rule 7 — the origin carries a classification at all. A sequence filed
        // before S003 has no activity to join, and continuing one would produce
        // a submission whose activity type is unknowable by construction. Kept
        // apart from the aggregate's "is it an opener?" rule because the two
        // failures are different: this one is about history, that one is about
        // shape.
        if (!origin.IsClassified)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.OriginatingSubmissionNotClassified);

        return SubmissionClassification.Continues(
            new OriginatingSubmission(
                origin.Id,
                origin.ApplicationId,
                origin.SequenceNumber,
                IsItselfAnOrigin: origin.OriginatingSubmissionId is null),
            command.SubmissionSubTypeId);
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
            // BUG-001: the tie-break in SQL, where a template id translates.
            // In memory it has no IComparable and threw on the second
            // candidate — the very tie the comment below anticipated.
            .OrderBy(t => t.Id)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var version = candidates
            // A tenant's own template shadows the platform-shared one, so the
            // choice of *template* is made first — picking the newest version
            // across all candidates would let a shared template outrank the
            // tenant's own.
            // Tenant-owned first — and then by id, because two tenant
            // templates matching one (authority, application type) would
            // otherwise let the database choose which one a filing binds to.
            // Seed data holds one, so the tie is unreachable today; this
            // ordering does not depend on that staying true.
            // Deterministic: the query above orders candidates by id in SQL
            // and this sort is stable (BUG-001).
            .OrderByDescending(t => t.TenantId != null)
            .Select(t => t.Versions
                // Within a template: published, effective today, newest wins.
                .Where(v => v.Status == TemplateVersionStatus.Published
                    && (v.EffectiveFrom is null || v.EffectiveFrom <= today)
                    && (v.EffectiveTo is null || v.EffectiveTo >= today))
                // Deterministic: a version number is unique within a
                // template — the unique index on (TemplateId, VersionNumber).
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefault())
            .FirstOrDefault(v => v is not null);

        return version?.Id;
    }
}
