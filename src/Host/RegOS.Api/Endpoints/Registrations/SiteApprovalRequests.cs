namespace RegOS.Api.Endpoints.Registrations;

/// <summary>
/// Records that a licence approves a manufacturing site, from a date.
/// </summary>
/// <param name="ApprovedOn">
/// Asked for rather than defaulted to today, for the reason a pack
/// authorisation's date is: a site routinely joins a licence by variation,
/// years after it was granted.
/// </param>
public sealed record ApproveSiteRequest(
    Guid RegistrationId,
    Guid OrganizationSiteId,
    DateOnly ApprovedOn);

public sealed record ApproveSiteResponse(Guid Id);
