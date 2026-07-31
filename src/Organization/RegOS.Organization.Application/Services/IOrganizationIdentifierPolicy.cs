using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Organization.Application.Services;

/// <summary>
/// The one rule about an organization identifier the aggregate cannot see: the
/// scheme has to be a real registry.
/// </summary>
/// <remarks>
/// A fourth parallel policy, and deliberately still not the extraction trigger.
/// <c>IContactCreationPolicy</c> settled the standard: <b>the trigger fires when
/// two of them need the same non-trivial rule, not merely when another
/// appears.</b> What this shares with the other three is one line of "does this
/// row exist", and a base class to hold that would couple four contexts to save
/// nothing.
/// <para>
/// It is not folded into <c>IOrganizationSiteCreationPolicy</c> either. That one
/// guards the creation of a site; this guards a change to an organization. They
/// happen to check the same table today, which is not the same as being the
/// same rule.
/// </para>
/// </remarks>
public interface IOrganizationIdentifierPolicy
{
    Task EnsureSchemeExistsAsync(
        IdentifierSchemeId schemeId,
        CancellationToken cancellationToken);
}

public static class OrganizationIdentifierRuleErrors
{
    public const string SchemeDoesNotExist =
        "That identifier scheme does not exist.";
}
