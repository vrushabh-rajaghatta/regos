using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Enums;
using RegOS.Submission.Domain.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Application.Commands.AttachProductDocument;

/// <summary>
/// Coordinates the Submission and Product Document aggregates. All
/// cross-aggregate validation happens here, before the aggregate is invoked;
/// the aggregate only enforces what it can see from its own state (draft,
/// no duplicates, display order).
/// </summary>
public sealed class AttachProductDocumentHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ISubmissionRepository _repository;

    public AttachProductDocumentHandler(
        RegOSDbContext dbContext,
        ISubmissionRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task<AttachProductDocumentResult> HandleAsync(
        AttachProductDocumentCommand command,
        CancellationToken cancellationToken)
    {
        // The Submission is the addressed resource — load it tracked, for
        // mutation. Absence is a 404.
        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        // The Product Document is supplied in the request body — an unknown id
        // is a business-rule violation (400), not a missing route resource.
        var productDocument = await _dbContext.ProductDocuments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == command.ProductDocumentId,
                cancellationToken);

        if (productDocument is null)
            throw new DomainException(
                SubmissionRuleErrors.ProductDocumentDoesNotExist);

        // Only governed (Active) assets may join a dossier.
        if (productDocument.Status != ProductDocumentStatus.Active)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.ProductDocumentNotActive);

        // Product ownership: Submission -> Application -> Product must match
        // the document's product. Guards against attaching another product's
        // document.
        var application = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == submission.ApplicationId,
                cancellationToken);

        if (application is null)
            throw new DomainException(
                SubmissionRuleErrors.ProductDocumentNotInSameProduct);

        if (productDocument.ProductId != application.ProductId)
            throw new DomainException(
                SubmissionRuleErrors.ProductDocumentNotInSameProduct);

        // The handler resolves the current version — the aggregate never
        // performs this lookup. An Active document always has one, but we
        // guard defensively.
        if (productDocument.CurrentVersionId is null)
            throw new DomainException(
                SubmissionRuleErrors.ProductDocumentHasNoCurrentVersion);

        var attachment = submission.AttachDocument(
            command.ProductDocumentId,
            productDocument.CurrentVersionId.Value);

        await _repository.UpdateAsync(submission, cancellationToken);

        return new AttachProductDocumentResult(attachment.Id);
    }
}
