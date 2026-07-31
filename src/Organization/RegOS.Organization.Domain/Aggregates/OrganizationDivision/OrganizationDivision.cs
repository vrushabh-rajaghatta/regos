using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.OrganizationDivision;

/// <summary>
/// A business unit within an organization — a sponsor's Regulatory Affairs
/// department, or the authority division that reviews a submission.
/// </summary>
/// <remarks>
/// <b>An aggregate root justified by stable independent identity rather than by
/// an immediate cross-registry directory.</b> That is an honest distinction from
/// <see cref="OrganizationSite"/> and <c>Contact</c>, which each earned root
/// status from a query users actually ask — <em>"which sites are in India?"</em>,
/// <em>"who is the QP?"</em>. Nobody asks for a directory of every division.
/// <para>
/// What it does have is the by-id reference: EPIC-006 will point an Application,
/// a Licence and an HA Meeting at <em>this division</em>, not at "the Sales
/// division". The platform has consistently made stable, independently
/// referenceable concepts roots before their richest read models existed —
/// Registration, OrganizationSite and Contact all shipped that way — and making
/// this a child now would save one table and one filter at the cost of
/// migrating ids, foreign keys, APIs and handlers once EPIC-006 arrives.
/// </para>
/// <para>
/// Status is an activation flag, like every other organizational entity: a
/// <see cref="StatusDate"/> and no history.
/// </para>
/// </remarks>
public sealed class OrganizationDivision : AggregateRoot<OrganizationDivisionId>
{
    public const int NameMaxLength = 250;

    private OrganizationDivision()
    {
    }

    public TenantId TenantId { get; private set; } = default!;

    public OrganizationId OrganizationId { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Acronym { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public DateOnly StatusDate { get; private set; }

    public static OrganizationDivision Create(
        TenantId tenantId,
        OrganizationId organizationId,
        string name,
        DateOnly statusDate,
        string? acronym = null)
    {
        if (tenantId is null)
            throw new DomainException(OrganizationDivisionErrors.TenantRequired);

        if (organizationId is null)
            throw new DomainException(
                OrganizationDivisionErrors.OrganizationRequired);

        if (statusDate == default)
            throw new DomainException(
                OrganizationDivisionErrors.StatusDateRequired);

        return new OrganizationDivision
        {
            Id = OrganizationDivisionId.New(),
            TenantId = tenantId,
            OrganizationId = organizationId,
            Name = NormalizeName(name),
            Acronym = string.IsNullOrWhiteSpace(acronym) ? null : acronym.Trim(),
            Status = OrganizationStatus.Active,
            StatusDate = statusDate,
        };
    }

    public void Rename(string? name) => Name = NormalizeName(name);

    public void Deactivate(DateOnly on)
    {
        if (Status == OrganizationStatus.Inactive)
            throw new BusinessRuleViolationException(
                OrganizationDivisionErrors.AlreadyInactive);

        Status = OrganizationStatus.Inactive;
        StatusDate = Dated(on);
    }

    public void Activate(DateOnly on)
    {
        if (Status == OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                OrganizationDivisionErrors.AlreadyActive);

        Status = OrganizationStatus.Active;
        StatusDate = Dated(on);
    }

    private static DateOnly Dated(DateOnly on)
        => on == default
            ? throw new DomainException(
                OrganizationDivisionErrors.StatusDateRequired)
            : on;

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(OrganizationDivisionErrors.NameRequired);

        var trimmed = name.Trim();

        return trimmed.Length > NameMaxLength
            ? throw new DomainException(OrganizationDivisionErrors.NameTooLong)
            : trimmed;
    }
}
