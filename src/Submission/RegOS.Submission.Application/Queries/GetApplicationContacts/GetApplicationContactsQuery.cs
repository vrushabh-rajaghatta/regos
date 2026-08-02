using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Application.Queries.GetApplicationContacts;

public sealed record GetApplicationContactsQuery(
    RegulatoryApplicationId ApplicationId);
