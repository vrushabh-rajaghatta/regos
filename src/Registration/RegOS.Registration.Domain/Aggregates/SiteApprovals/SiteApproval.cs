using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Aggregates.SiteApprovals;

/// <summary>
/// A licence approving one site, from a date.
/// </summary>
/// <remarks>
/// <b>The second occurrence of <em>licence + thing + date</em>, and it is
/// copied rather than abstracted</b>
/// ([ADR-018](../../../../docs/adr/ADR-018-rule-of-three.md)).
/// <see cref="PackAuthorisations.PackAuthorisation"/> is the first, and the two
/// are near-identical by shape: a tenant, two ids, a business date, a recorded
/// timestamp. **Two occurrences is a pattern; three is the point at which to
/// evaluate whether a shared abstraction is earned** — and 010b's own retro is
/// the reminder that the evaluation may correctly return *no*.
/// <para>
/// <b>The reason is identical too, which is what makes it a pattern rather than
/// a coincidence.</b> A site joins a licence <em>by variation</em>, routinely
/// years after approval: a marketing authorisation granted in 2021 that added a
/// secondary packaging site in 2024 has two dates, and only one of them is the
/// registration's. A foreign key on either aggregate can carry neither.
/// </para>
/// <para>
/// <b>Its own root rather than a collection on
/// <see cref="Registration.Registration"/>.</b> The question asked of it —
/// <em>"is the site we manufacture at approved here?"</em> — starts at the
/// market and reaches licences, so a collection would mean loading every
/// registration to answer it. It also keeps <c>Registration</c> genuinely
/// untouched, which is what let EPIC-005's work stay closed.
/// </para>
/// <para>
/// <b>No new context dependency was needed.</b> <c>Registration.Domain</c>
/// already referenced <c>Organization.Domain</c> — the holder organization made
/// sure of that — which is the difference between this and
/// <see href="../../../../docs/adr/ADR-063-where-a-product-is-made-is-a-product-fact.md">ADR-063</see>'s
/// side of the epic.
/// </para>
/// <para>
/// <b>What it does not record.</b> Not whether the site is <em>fit</em> to
/// perform the work — that is a quality system's statement, and EPIC-008's. Not
/// which operation the licence approves it <em>for</em>: an authorisation names
/// its sites, and matching them to what those sites actually do is the
/// comparison S004 derives rather than a fact either side holds.
/// </para>
/// </remarks>
public sealed class SiteApproval : AggregateRoot<SiteApprovalId>
{
    private SiteApproval()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>The licence that names the site.</summary>
    public RegistrationId RegistrationId { get; private set; }

    /// <summary>The site it approves.</summary>
    public OrganizationSiteId OrganizationSiteId { get; private set; } = default!;

    /// <summary>
    /// The business date the site was added to this licence.
    /// </summary>
    /// <remarks>
    /// <b>The fact a foreign key could not carry</b>, for the second time in
    /// this codebase. A licence approved in 2021 whose packaging site was added
    /// in 2024 has two dates, and a filing that quotes the wrong one is wrong.
    /// </remarks>
    public DateOnly ApprovedOn { get; private set; }

    public DateTime RecordedOnUtc { get; private set; }

    public static SiteApproval Create(
        TenantId tenantId,
        RegistrationId registrationId,
        OrganizationSiteId organizationSiteId,
        DateOnly approvedOn)
    {
        if (tenantId is null)
            throw new DomainException(SiteApprovalErrors.TenantRequired);

        if (organizationSiteId is null)
            throw new DomainException(SiteApprovalErrors.SiteRequired);

        if (approvedOn == default)
            throw new DomainException(SiteApprovalErrors.ApprovedOnRequired);

        return new SiteApproval
        {
            Id = SiteApprovalId.New(),
            TenantId = tenantId,
            RegistrationId = registrationId,
            OrganizationSiteId = organizationSiteId,
            ApprovedOn = approvedOn,
            RecordedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Corrects the date the site was added.
    /// </summary>
    /// <remarks>
    /// The pair it names is immutable: a different site or a different licence
    /// is a different approval, and editing one into another would leave no way
    /// to tell a correction from a replacement — the same call
    /// <c>PackAuthorisation.Correct</c> makes about the pair it names.
    /// </remarks>
    public void Correct(DateOnly approvedOn)
    {
        if (approvedOn == default)
            throw new DomainException(SiteApprovalErrors.ApprovedOnRequired);

        ApprovedOn = approvedOn;
    }
}
