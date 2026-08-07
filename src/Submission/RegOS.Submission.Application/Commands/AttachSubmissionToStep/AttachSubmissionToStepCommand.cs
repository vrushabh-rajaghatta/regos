using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AttachSubmissionToStep;

/// <param name="ProcessStepId">
/// Null clears the link. Clearing is always permitted — an attachment is
/// descriptive, so removing one changes discoverability and nothing else
/// (ADR-065 I9).
/// </param>
public sealed record AttachSubmissionToStepCommand(
    SubmissionId SubmissionId,
    ProcessStepId? ProcessStepId);
