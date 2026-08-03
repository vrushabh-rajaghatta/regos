using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Substances;

/// <summary>
/// A scientific entity that exists independently of any product — paracetamol
/// is paracetamol whether or not a regulator, a tenant or RegOS exists.
/// </summary>
/// <remarks>
/// <b>A substance is a fact; an ingredient is the role it plays in one product</b>
/// (ADR-058). The split is what makes the question <em>"which of our products
/// contain substance X?"</em> answerable: a name repeated on each product's
/// composition can only be asked forwards, and asking it backwards would mean
/// matching on strings.
/// <para>
/// <b>Shared plus extensible</b> — a null <see cref="TenantId"/> is a substance
/// the platform ships, visible to everyone; a set one is a tenant's own. The
/// shape is <c>AuthorityDivision</c>'s and the reason is not: that one is
/// extensible because <em>no authoritative source exists</em>, this one because
/// an authoritative source exists and <b>the tenant's molecule is not in it
/// yet</b>. An innovator holds a compound before INN assignment. The second
/// reason resolves itself when licensed terminology arrives; the first never
/// does (ADR-058 §2).
/// </para>
/// <para>
/// <b>A shared substance cannot be mutated, and in this story that is
/// structural rather than guarded.</b> No write path modifies an existing row
/// at all — <see cref="ISubstanceRepository"/> adds and nothing else, and the
/// only factory reachable from the API stamps the calling tenant. Steward
/// editing, lifecycle and change control are EPIC-012's, and the guard belongs
/// on the first mutation that exists rather than ahead of it.
/// </para>
/// <para>
/// <b>Two name fields, not RIM's three.</b> The source sheet carries
/// <c>Substance (API)</c>, <c>Substance Name</c> and <c>INN</c>, all required
/// and all typed the same, with no stated distinction. <see cref="Name"/> holds
/// the preferred scientific name and <see cref="Inn"/> the WHO International
/// Nonproprietary Name where one has been assigned. <see cref="Inn"/> is
/// nullable because <b>a proprietary pre-INN compound is exactly the case this
/// aggregate's tenant half exists to serve</b> — the field's absence is
/// meaningful, not missing (EPIC-010a D7).
/// </para>
/// <para>
/// <b>No <c>IsActive</c>.</b> The epic's table lists one; the founder scoped
/// lifecycle management to EPIC-012, which leaves nothing in this story able to
/// write it. A persistent property with no acquisition path is the defect
/// EPIC-007a spent three findings on, so it arrives with the capability that
/// sets it — an additive migration.
/// </para>
/// </remarks>
public sealed class Substance : AggregateRoot<SubstanceId>
{
    public const int NameMaxLength = 250;
    public const int IdentifierMaxLength = 50;
    public const int MolecularFormulaMaxLength = 100;
    public const int DescriptionMaxLength = 2000;

    private Substance()
    {
    }

    /// <summary>
    /// Null for a substance the platform ships, set for a tenant's own.
    /// </summary>
    public TenantId? TenantId { get; private set; }

    /// <summary>
    /// True while nobody owns this row — the catalogue everyone sees and
    /// nobody edits.
    /// </summary>
    public bool IsShared => TenantId is null;

    /// <summary>The preferred scientific name — the one displayed.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// The WHO International Nonproprietary Name, where one has been assigned.
    /// </summary>
    public string? Inn { get; private set; }

    /// <summary>The structural axis (EPIC-010a D7).</summary>
    public CodedConcept SubstanceClass { get; private set; } = default!;

    /// <summary>The origin axis (EPIC-010a D7).</summary>
    public CodedConcept SubstanceType { get; private set; } = default!;

    public string? CasNumber { get; private set; }

    /// <summary>
    /// The GSRS seam. Null on every seeded row, because RegOS does not hold
    /// GSRS and a populated one would claim otherwise (ADR-058 §6); settable by
    /// a tenant that knows its own compound's code.
    /// </summary>
    public string? UniiCode { get; private set; }

    public string? MolecularFormula { get; private set; }

    /// <summary>RIM's <em>Chem/Bio Description</em>.</summary>
    public string? Description { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// A substance the platform ships. Deterministic id, no owner.
    /// </summary>
    /// <remarks>
    /// Named apart from <see cref="CreateForTenant"/> so that producing a
    /// shared row is something a caller has to ask for by name. The data
    /// initializer is the only caller; no API path can reach it.
    /// </remarks>
    public static Substance Seed(
        SubstanceId id,
        string name,
        string? inn,
        CodedConcept substanceClass,
        CodedConcept substanceType,
        string? casNumber = null,
        string? molecularFormula = null,
        string? description = null)
    {
        var substance = Build(
            tenantId: null,
            name,
            inn,
            substanceClass,
            substanceType,
            casNumber,
            uniiCode: null,
            molecularFormula,
            description);

        substance.Id = id;

        return substance;
    }

    /// <summary>
    /// A tenant's own compound, sitting beside the shared catalogue and visible
    /// only to them.
    /// </summary>
    public static Substance CreateForTenant(
        TenantId tenantId,
        string name,
        string? inn,
        CodedConcept substanceClass,
        CodedConcept substanceType,
        string? casNumber = null,
        string? uniiCode = null,
        string? molecularFormula = null,
        string? description = null)
    {
        if (tenantId is null)
            throw new DomainException(SubstanceErrors.TenantRequired);

        var substance = Build(
            tenantId,
            name,
            inn,
            substanceClass,
            substanceType,
            casNumber,
            uniiCode,
            molecularFormula,
            description);

        substance.Id = SubstanceId.New();

        return substance;
    }

    private static Substance Build(
        TenantId? tenantId,
        string name,
        string? inn,
        CodedConcept substanceClass,
        CodedConcept substanceType,
        string? casNumber,
        string? uniiCode,
        string? molecularFormula,
        string? description)
    {
        if (substanceClass is null)
            throw new DomainException(SubstanceErrors.ClassRequired);

        if (substanceType is null)
            throw new DomainException(SubstanceErrors.TypeRequired);

        return new Substance
        {
            TenantId = tenantId,
            Name = ValidatedName(name),
            Inn = Optional(inn, NameMaxLength, SubstanceErrors.InnTooLong),
            SubstanceClass = substanceClass,
            SubstanceType = substanceType,
            CasNumber = Optional(
                casNumber, IdentifierMaxLength, SubstanceErrors.CasNumberTooLong),
            UniiCode = Optional(
                uniiCode, IdentifierMaxLength, SubstanceErrors.UniiCodeTooLong),
            MolecularFormula = Optional(
                molecularFormula,
                MolecularFormulaMaxLength,
                SubstanceErrors.MolecularFormulaTooLong),
            Description = Optional(
                description,
                DescriptionMaxLength,
                SubstanceErrors.DescriptionTooLong),
            CreatedOn = DateTime.UtcNow
        };
    }

    private static string ValidatedName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(SubstanceErrors.NameRequired);

        var trimmed = value.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new DomainException(SubstanceErrors.NameTooLong);

        return trimmed;
    }

    /// <remarks>
    /// Blank collapses to null rather than being stored: an empty CAS number
    /// and an absent one are the same fact, and only one of them can be
    /// queried for.
    /// </remarks>
    private static string? Optional(string? value, int maxLength, string tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new DomainException(tooLongError);

        return trimmed;
    }
}
