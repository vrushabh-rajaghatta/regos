using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

/// <summary>
/// A physical location an organization operates — a plant, a testing laboratory,
/// a health authority's office.
/// </summary>
/// <remarks>
/// An aggregate root rather than a child of <see cref="Organization"/>, on the
/// same test EPIC-005 applied to Registration: users do not only ask <em>"load
/// Organization X and inspect its sites"</em>, they ask <em>"which manufacturing
/// sites do we have in India?"</em>. More decisively, other aggregates will
/// reference a site <b>by id</b> — a licence naming approved manufacturers, an
/// ingredient naming its manufacturing source — which is the aggregate-root
/// signal. As a child it would force a lock on the organization for every site
/// change.
/// <para>
/// <b>Status is an activation flag, not a lifecycle</b>, so there is no history
/// child here. Active/Inactive answers <em>do we still use this place?</em> —
/// current operability, the same concept <see cref="Organization"/> and Product
/// already carry without history. Contrast <c>Registration</c>, whose status
/// records dated positions an authority took and therefore earns
/// <c>RegistrationStatusEntry</c>. <see cref="StatusDate"/> is the proportionate
/// answer where the date still matters.
/// </para>
/// </remarks>
public sealed class OrganizationSite : AggregateRoot<OrganizationSiteId>
{
    public const int NameMaxLength = 250;

    private readonly List<SiteIdentifier> _identifiers = [];

    private OrganizationSite()
    {
    }

    /// <summary>
    /// The tenant whose registry this site belongs to. Sites carry their own
    /// tenant and their own fail-closed filter rather than inheriting the
    /// organization's — they are a root, reachable directly through the
    /// directory, not only through a filtered parent (ADR-031/032).
    /// </summary>
    public TenantId TenantId { get; private set; } = default!;

    public OrganizationId OrganizationId { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? NameNativeLanguage { get; private set; }

    public OrganizationSiteType Type { get; private set; }

    public PostalAddress Address { get; private set; } = default!;

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public OrganizationStatus Status { get; private set; }

    /// <summary>
    /// The business date the site opened, or closed. Supplied rather than read
    /// from the clock, so a site recorded today can say it has operated since
    /// 2014.
    /// </summary>
    public DateOnly StatusDate { get; private set; }

    /// <summary>
    /// Every registry identifier this site holds. A US plant routinely carries
    /// both an FEI and a DUNS number, and they are peers.
    /// </summary>
    public IReadOnlyCollection<SiteIdentifier> Identifiers
        => _identifiers.AsReadOnly();

    public static OrganizationSite Create(
        TenantId tenantId,
        OrganizationId organizationId,
        string name,
        OrganizationSiteType type,
        PostalAddress address,
        DateOnly statusDate,
        string? nameNativeLanguage = null,
        string? email = null,
        string? phone = null)
    {
        if (tenantId is null)
            throw new DomainException(OrganizationSiteErrors.TenantRequired);

        if (organizationId is null)
            throw new DomainException(
                OrganizationSiteErrors.OrganizationRequired);

        if (address is null)
            throw new DomainException(OrganizationSiteErrors.AddressRequired);

        if (statusDate == default)
            throw new DomainException(
                OrganizationSiteErrors.StatusDateRequired);

        return new OrganizationSite
        {
            Id = OrganizationSiteId.New(),
            TenantId = tenantId,
            OrganizationId = organizationId,
            Name = NormalizeName(name),
            NameNativeLanguage = string.IsNullOrWhiteSpace(nameNativeLanguage)
                ? null
                : nameNativeLanguage.Trim(),
            Type = Validated(type),
            Address = address,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Status = OrganizationStatus.Active,
            StatusDate = statusDate,
        };
    }

    public void Rename(string? name) => Name = NormalizeName(name);

    public void Reclassify(OrganizationSiteType type) => Type = Validated(type);

    public void Relocate(PostalAddress address)
        => Address = address
            ?? throw new DomainException(OrganizationSiteErrors.AddressRequired);

    /// <summary>
    /// Records an identifier this site is known by in some registry.
    /// </summary>
    /// <remarks>
    /// One per scheme: a site has a single FEI, and a second would mean one of
    /// them is wrong rather than that the site has two. Different schemes
    /// coexist freely, which is the whole reason this is a collection.
    /// </remarks>
    public SiteIdentifier AddIdentifier(IdentifierSchemeId schemeId, string value)
    {
        if (_identifiers.Any(x => x.SchemeId == schemeId))
            throw new BusinessRuleViolationException(
                OrganizationSiteErrors.IdentifierSchemeAlreadyRecorded);

        var identifier = new SiteIdentifier(
            SiteIdentifierId.New(), schemeId, value);

        _identifiers.Add(identifier);

        return identifier;
    }

    /// <summary>
    /// Removes an identifier. Unlike a registration's history, these are not a
    /// record of events — they are current facts about the site, and a mistyped
    /// FEI should be correctable.
    /// </summary>
    public void RemoveIdentifier(SiteIdentifierId identifierId)
    {
        var identifier = _identifiers.FirstOrDefault(x => x.Id == identifierId)
            ?? throw new NotFoundException(
                OrganizationSiteErrors.IdentifierNotFound);

        _identifiers.Remove(identifier);
    }

    /// <summary>
    /// Retires the site. It stays readable and everything that references it is
    /// untouched — the same meaning deactivation has for an organization: "do
    /// not start anything new here", not "pretend it never existed".
    /// </summary>
    public void Deactivate(DateOnly on)
    {
        if (Status == OrganizationStatus.Inactive)
            throw new BusinessRuleViolationException(
                OrganizationSiteErrors.AlreadyInactive);

        Status = OrganizationStatus.Inactive;
        StatusDate = Dated(on);
    }

    public void Activate(DateOnly on)
    {
        if (Status == OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                OrganizationSiteErrors.AlreadyActive);

        Status = OrganizationStatus.Active;
        StatusDate = Dated(on);
    }

    private static DateOnly Dated(DateOnly on)
        => on == default
            ? throw new DomainException(
                OrganizationSiteErrors.StatusDateRequired)
            : on;

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(OrganizationSiteErrors.NameRequired);

        var trimmed = name.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(OrganizationSiteErrors.NameTooLong)
            : trimmed;
    }

    // Model binding happily turns {"type": 99} into an OrganizationSiteType.
    private static OrganizationSiteType Validated(OrganizationSiteType type)
        => Enum.IsDefined(type)
            ? type
            : throw new DomainException(OrganizationSiteErrors.TypeInvalid);
}
