using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AssignSubmissionRole;

/// <summary>
/// Names a person on a draft submission (ADR-048).
/// </summary>
public sealed record AssignSubmissionRoleCommand(
    SubmissionId SubmissionId,
    ContactId ContactId,
    ContactRoleId RoleId);
