namespace RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

public sealed record ListRegulatoryApplicationsResult(
    IReadOnlyList<RegulatoryApplicationInfo> Applications);
