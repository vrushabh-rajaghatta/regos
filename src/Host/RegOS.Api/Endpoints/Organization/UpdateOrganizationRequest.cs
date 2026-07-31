using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

/// <param name="Acronym">Short form the company trades under — "DML".</param>
/// <param name="NameNativeLanguage">
/// The legal name in local script, for a company whose filings are not in
/// English. Omitted or blank clears it.
/// </param>
public sealed record UpdateOrganizationRequest(
    string? LegalName,
    OrganizationType Type,
    string? Acronym = null,
    string? NameNativeLanguage = null);
