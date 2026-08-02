using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Interaction.Application.Queries.ListCorrespondence;

/// <summary>
/// The ways a user narrows the correspondence list — <em>"what has Health
/// Canada sent us?"</em>, <em>"what came in about this application?"</em>,
/// <em>"did they acknowledge sequence 0003?"</em>. All optional; the unfiltered
/// list is the inbox.
/// </summary>
/// <remarks>
/// <see cref="SubmissionId"/> exposes an anchor <c>HaCorrespondence</c> has
/// carried since EPIC-006 S001 but never offered as a filter. It is what lets a
/// submission page show what the authority said <b>without the Submission
/// context knowing anything about correspondence</b>: the answer is composed by
/// the caller from two projections, rather than by a dependency between two
/// bounded contexts (ADR-046).
/// </remarks>
public sealed record ListCorrespondenceQuery(
    AuthorityId? AuthorityId = null,
    CorrespondenceTypeId? CorrespondenceTypeId = null,
    CorrespondenceDirection? Direction = null,
    RegulatoryApplicationId? RegulatoryApplicationId = null,
    SubmissionId? SubmissionId = null);
