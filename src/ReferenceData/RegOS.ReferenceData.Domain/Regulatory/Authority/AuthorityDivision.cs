using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Regulatory.Authority;

/// <summary>
/// A unit inside a health authority — FDA's <em>Division of Neurology 1</em>,
/// Health Canada's <em>Therapeutic Products Directorate</em>.
/// </summary>
/// <remarks>
/// <b>Platform-seeded, tenant-augmentable.</b> A null <see cref="TenantId"/> is
/// a division the platform ships, visible to everyone; a set one is a tenant's
/// own, visible only to them. The same physical shape as <c>ContactRole</c>,
/// but arrived at by a different argument and worth stating, because the
/// conclusion should not be pattern-matched.
/// <para>
/// It is not that tenants disagree about what a division means. It is that
/// <b>RegOS has no authoritative source for the world's authority
/// divisions</b> — FDA alone has around thirty review divisions, Health Canada
/// organises into directorates and bureaux, and the EMA works through
/// committees rather than divisions at all. A partial seed with no way to add
/// is a feature that blocks the first user whose division is missing. The
/// tenant is not extending the meaning of the FDA; they are recording
/// <em>"this is the review division we actually deal with"</em>. That is a
/// representational gap, not a preference.
/// </para>
/// <para>
/// <b>No hierarchy, and its absence is the decision.</b> FDA's structure runs
/// four levels deep and the EMA's is not a tree of divisions at all — but none
/// of the questions this exists to answer needs a parent. <em>"Show me
/// everything under the Office of Neuroscience"</em> would be the evidence; the
/// structure merely existing is not. A nullable <c>ParentId</c> is an additive
/// migration whenever that question is actually asked.
/// </para>
/// <para>
/// <b>No contacts either.</b> Regulatory correspondence is addressed to a
/// division or a docket, not to a person. People become load-bearing for
/// meeting attendees — EPIC-006 S005 — and belong to that story's evidence
/// rather than this one's symmetry.
/// </para>
/// </remarks>
public sealed class AuthorityDivision
{
    public const int NameMaxLength = 250;

    private AuthorityDivision()
    {
    }

    public AuthorityDivisionId Id { get; private set; }

    /// <summary>The authority this division belongs to. Immutable.</summary>
    public AuthorityId AuthorityId { get; private set; }

    /// <summary>Null for a platform division; set for a tenant's own.</summary>
    public TenantId? TenantId { get; private set; }

    public string Name { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public static AuthorityDivision Create(
        AuthorityDivisionId id,
        AuthorityId authorityId,
        string name,
        TenantId? tenantId = null)
    {
        if (authorityId == default)
            throw new DomainException(AuthorityDivisionErrors.AuthorityRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(AuthorityDivisionErrors.NameRequired);

        var trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new DomainException(AuthorityDivisionErrors.NameTooLong);

        return new AuthorityDivision
        {
            Id = id,
            AuthorityId = authorityId,
            TenantId = tenantId,
            Name = trimmed,
            IsActive = true
        };
    }

    public static AuthorityDivision Create(
        AuthorityId authorityId,
        string name,
        TenantId? tenantId = null)
        => Create(AuthorityDivisionId.New(), authorityId, name, tenantId);
}
