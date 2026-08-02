using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.CreateSubmission;

public sealed record CreateSubmissionCommand(
    RegulatoryApplicationId ApplicationId,
    string Title,
    SubmissionFormat Format);
