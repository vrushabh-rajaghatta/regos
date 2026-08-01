using RegOS.Interaction.Domain.Inspections;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.Interaction.Application.Commands.BeginInspection;

public sealed record BeginInspectionCommand(
    AuthorityId AuthorityId,
    string Title,
    InspectionStatus InitialStatus,
    DateOnly OccurredOn,
    OrganizationSiteId? OrganizationSiteId,
    DateOnly? ScheduledFor);
