using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Services;

/// <summary>
/// The rules a contact must satisfy that the aggregate cannot see.
/// </summary>
/// <remarks>
/// The <b>third</b> parallel creation policy, after
/// <c>RegistrationCreationPolicy</c> and <c>OrganizationSiteCreationPolicy</c> —
/// which is the Rule-of-Three trigger recorded in the first of them. It is
/// deliberately not acted on here: the three check overlapping but different
/// sets of things (a registration checks authority-belongs-to-country, a site
/// checks identifier schemes, this checks site-belongs-to-organization), and
/// what they share is two lines of "does this row exist". Extracting a base
/// class to save those would couple three bounded contexts. <b>The trigger fires
/// when two of them need the same non-trivial rule, not merely when a third
/// appears.</b>
/// </remarks>
public interface IContactCreationPolicy
{
    Task EnsureCanCreateAsync(
        OrganizationId organizationId,
        OrganizationSiteId? siteId,
        CountryId? countryId,
        IReadOnlyCollection<ContactRoleId> roleIds,
        CancellationToken cancellationToken);
}

public static class ContactRuleErrors
{
    public const string OrganizationDoesNotExist =
        "Organization does not exist.";

    public const string OrganizationInactive =
        "That organization is not active.";

    public const string SiteDoesNotExist = "Site does not exist.";

    public const string SiteNotForOrganization =
        "That site belongs to a different organization.";

    public const string CountryDoesNotExist = "Country does not exist.";

    public const string RoleDoesNotExist = "One of the roles does not exist.";

    public const string ContactDoesNotExist = "Contact does not exist.";
}
