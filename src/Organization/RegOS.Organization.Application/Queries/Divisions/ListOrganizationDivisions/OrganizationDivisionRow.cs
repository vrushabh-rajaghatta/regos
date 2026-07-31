namespace RegOS.Organization.Application.Queries.Divisions.ListOrganizationDivisions;

public sealed record OrganizationDivisionRow(
    Guid DivisionId,
    string Name,
    string? Acronym,
    string Status,
    DateOnly StatusDate);
