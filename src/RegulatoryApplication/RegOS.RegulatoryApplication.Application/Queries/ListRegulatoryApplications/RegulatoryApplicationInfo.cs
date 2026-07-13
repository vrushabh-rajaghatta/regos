namespace RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

public sealed record RegulatoryApplicationInfo(
    Guid Id,
    string Name,
    string? ApplicationNumber,
    string Status);
