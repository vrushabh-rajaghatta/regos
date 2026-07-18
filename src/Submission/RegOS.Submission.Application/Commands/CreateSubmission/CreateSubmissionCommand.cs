using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Application.Commands.CreateSubmission;

public sealed record CreateSubmissionCommand(
    RegulatoryApplicationId ApplicationId,
    SubmissionTypeId SubmissionTypeId,
    string Title);
