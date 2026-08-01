using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Interaction.Application.Commands.RecordCorrespondence;

/// <summary>
/// Logs a letter that has already happened. The tenant is ambient (who is
/// asking); everything here is a fact about the letter itself.
/// </summary>
public sealed record RecordCorrespondenceCommand(
    AuthorityId AuthorityId,
    CorrespondenceTypeId CorrespondenceTypeId,
    AuthorityDivisionId? AuthorityDivisionId,
    CorrespondenceDirection Direction,
    string Subject,
    DateOnly OccurredOn,
    DateOnly? ResponseDueOn,
    string? AuthorityReference,
    RegulatoryApplicationId? RegulatoryApplicationId,
    SubmissionId? SubmissionId,
    RegistrationId? RegistrationId);
