using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.ApproveSite;

/// <summary>
/// Records that a licence approves a manufacturing site, from a date.
/// </summary>
/// <remarks>
/// <b>Recording what an authority decided, not deciding it.</b> The verb
/// matches <c>AuthorisePack</c> beside it for consistency; the act in both cases
/// is writing down a statement the licence already makes.
/// </remarks>
/// <param name="ApprovedOn">
/// <b>Supplied, never read from the clock.</b> A licence granted in 2021 that
/// added a secondary packaging site in 2024 by variation has two dates, and only
/// one of them is the registration's.
/// </param>
public sealed record ApproveSiteCommand(
    RegistrationId RegistrationId,
    OrganizationSiteId OrganizationSiteId,
    DateOnly ApprovedOn);

public sealed record ApproveSiteResult(Guid SiteApprovalId);
