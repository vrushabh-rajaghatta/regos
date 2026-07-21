using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Organization.Application.Commands.ActivateOrganization;

public sealed record ActivateOrganizationCommand(OrganizationId Id);
