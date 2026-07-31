using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Services;

/// <summary>
/// The rules a site must satisfy that the aggregate cannot see — they concern
/// records in other tables.
/// </summary>
/// <remarks>
/// A parallel policy rather than a shared one, on the same reasoning recorded in
/// <c>IRegistrationCreationPolicy</c>: the checks look similar across contexts
/// but the sets diverge, and extracting a common one would couple contexts meant
/// to stay independent. <b>The third occurrence triggers extraction, not the
/// fourth</b> — with <c>RegistrationCreationPolicy</c> this is the second.
/// </remarks>
public interface IOrganizationSiteCreationPolicy
{
    Task EnsureCanCreateAsync(
        OrganizationId organizationId,
        CountryId countryId,
        IReadOnlyCollection<IdentifierSchemeId> schemeIds,
        CancellationToken cancellationToken);
}

public static class OrganizationSiteRuleErrors
{
    public const string OrganizationDoesNotExist =
        "Organization does not exist.";

    public const string OrganizationInactive =
        "That organization is not active.";

    public const string CountryDoesNotExist =
        "Country does not exist.";

    public const string IdentifierSchemeDoesNotExist =
        "One of the identifier schemes does not exist.";

    public const string SiteDoesNotExist =
        "Site does not exist.";
}
