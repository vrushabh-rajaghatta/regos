using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.DeactivateOrganization;

public sealed record DeactivateOrganizationCommand(OrganizationId Id);
