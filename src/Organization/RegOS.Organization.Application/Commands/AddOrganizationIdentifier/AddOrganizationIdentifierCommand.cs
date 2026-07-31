using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Commands.AddOrganizationIdentifier;

/// <summary>
/// Records an identifier a registry has issued to this company — a DUNS number,
/// a VAT number, an EU ORG-ID.
/// </summary>
public sealed record AddOrganizationIdentifierCommand(
    OrganizationId OrganizationId,
    IdentifierSchemeId SchemeId,
    string Value);
