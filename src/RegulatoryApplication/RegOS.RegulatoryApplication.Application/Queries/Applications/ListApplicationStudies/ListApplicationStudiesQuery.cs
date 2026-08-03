using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Queries.Applications.ListApplicationStudies;

/// <summary>"Which studies support this filing?"</summary>
public sealed record ListApplicationStudiesQuery(
    RegulatoryApplicationId ApplicationId);
