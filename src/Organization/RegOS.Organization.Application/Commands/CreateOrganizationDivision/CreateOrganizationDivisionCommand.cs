using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.CreateOrganizationDivision;

/// <param name="StatusDate">The business date the division was established.</param>
public sealed record CreateOrganizationDivisionCommand(
    OrganizationId OrganizationId,
    string Name,
    DateOnly StatusDate,
    string? Acronym = null);
