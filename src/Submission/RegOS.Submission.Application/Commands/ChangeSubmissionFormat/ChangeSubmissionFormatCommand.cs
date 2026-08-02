using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.ChangeSubmissionFormat;

/// <summary>
/// Changes what a draft submission will be rendered as.
/// </summary>
/// <remarks>
/// There is no matching command for a published sequence, and deliberately so:
/// its format is a fact about a filing already made (ADR-047). The aggregate
/// refuses it either way — this is simply the API declining to offer the door.
/// </remarks>
public sealed record ChangeSubmissionFormatCommand(
    SubmissionId SubmissionId,
    SubmissionFormat Format);
