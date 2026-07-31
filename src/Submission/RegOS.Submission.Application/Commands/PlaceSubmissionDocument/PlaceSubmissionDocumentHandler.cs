using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Placement;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.PlaceSubmissionDocument;

/// <summary>
/// Places a document into the dossier structure, or clears its placement.
/// </summary>
/// <remarks>
/// This is the endpoint drag-and-drop will use in STORY-004, which is why it
/// expresses the destination rather than a delta.
/// </remarks>
public sealed class PlaceSubmissionDocumentHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ISubmissionRepository _repository;

    public PlaceSubmissionDocumentHandler(
        RegOSDbContext dbContext,
        ISubmissionRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task HandleAsync(
        PlaceSubmissionDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        if (command.TemplateSectionId is { } sectionId)
        {
            await SectionPlacementPolicy.EnsureSectionIsInBoundBlueprintAsync(
                _dbContext, submission, sectionId, cancellationToken);

            // The aggregate rejects an id that is not attached to *this*
            // submission — placement must never become an attach-by-reference
            // back door around the ownership and active-status rules.
            submission.PlaceDocument(command.SubmissionDocumentId, sectionId);
        }
        else
        {
            submission.ClearPlacement(command.SubmissionDocumentId);
        }

        await _repository.UpdateAsync(submission, cancellationToken);
    }
}
