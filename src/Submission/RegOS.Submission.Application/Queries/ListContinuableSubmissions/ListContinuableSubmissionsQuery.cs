using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Application.Queries.ListContinuableSubmissions;

/// <summary>
/// The published sequences in an application that a new one could continue.
/// </summary>
public sealed record ListContinuableSubmissionsQuery(
    RegulatoryApplicationId ApplicationId);
