using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Interaction.Application.Queries.ListCorrespondence;

/// <summary>
/// The four ways a user narrows the correspondence list — <em>"what has Health
/// Canada sent us?"</em>, <em>"what came in about this application?"</em>. All
/// optional; the unfiltered list is the inbox.
/// </summary>
public sealed record ListCorrespondenceQuery(
    AuthorityId? AuthorityId = null,
    CorrespondenceTypeId? CorrespondenceTypeId = null,
    CorrespondenceDirection? Direction = null,
    RegulatoryApplicationId? RegulatoryApplicationId = null);
