using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Organization;

/// <summary>
/// A registry that issues identifiers for organizations and their sites —
/// DUNS, FEI, EU ORG-ID, SPL.
/// </summary>
/// <remarks>
/// Reference data rather than an enum, and the test is the one EPIC-005 used on
/// <c>RegistrationStatus</c>: does it drive <em>what may happen</em>? A status
/// does — it decides what a registration may become. A scheme does not. It is
/// <b>vocabulary</b>: jurisdiction-specific, externally governed, occasionally
/// extended, and an enum would need a deployment to add one.
/// <para>
/// Global and unfiltered, like <c>Country</c> and <c>Authority</c> — these are
/// world facts, not a tenant's own list. If a customer ever needs a private
/// internal scheme, the move is to the shared-plus-extensible shape
/// (<c>DocumentType</c>): a nullable <c>TenantId</c> and one filter, with the
/// seeded rows keeping a null tenant.
/// </para>
/// </remarks>
public sealed class IdentifierScheme
{
    private IdentifierScheme()
    {
    }

    public IdentifierSchemeId Id { get; private set; }

    /// <summary>Short form the industry uses: <c>DUNS</c>, <c>FEI</c>.</summary>
    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    /// <summary>
    /// Who issues it — "Dun &amp; Bradstreet", "US FDA". Not a link to
    /// <c>Authority</c>: most issuers are not regulators.
    /// </summary>
    public string Issuer { get; private set; } = default!;

    public static IdentifierScheme Create(
        IdentifierSchemeId id,
        string code,
        string name,
        string issuer)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(IdentifierSchemeErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(IdentifierSchemeErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(issuer))
            throw new DomainException(IdentifierSchemeErrors.IssuerRequired);

        return new IdentifierScheme
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Issuer = issuer.Trim(),
        };
    }
}

public static class IdentifierSchemeErrors
{
    public const string CodeRequired = "An identifier scheme needs a code.";

    public const string NameRequired = "An identifier scheme needs a name.";

    public const string IssuerRequired = "An identifier scheme needs an issuer.";
}
