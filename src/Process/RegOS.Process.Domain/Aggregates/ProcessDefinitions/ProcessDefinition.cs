using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// The authoritative, versioned description of a regulatory process — <b>"Playbook"
/// on screen.</b>
/// </summary>
/// <remarks>
/// <b>A definition, not a template</b>
/// ([ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// decision 7). A template is something you copy and edit, and copies diverge
/// freely. A definition is something you conform to: it is versioned, published,
/// and a plan is pinned to one of its versions permanently. The second is what
/// this is, which is why RIM's own word — <c>Process Plan Template</c> — is
/// deliberately not used.
/// <para>
/// <b>The root owns version numbering and permits one open draft</b>, which is the
/// shape <c>RegulatoryTemplate</c> already proved for dossier blueprints. The two
/// are knowingly <b>mirrored rather than shared</b> (ADR-065 decision 1): a
/// section tree carrying eCTD folders and a step graph carrying offsets are the
/// same lifecycle over very different payloads, and ADR-018 wants a third
/// demonstrated need before the lifecycle is extracted.
/// </para>
/// <para>
/// <b>Scoped by country, authority and application type.</b> What you must do to
/// open an IND with FDA is not what you must do to file an MAA in the EU, and the
/// three together are what selects a playbook. Authority is a scope dimension from
/// day one because adding it later would be a migration on a shipped table.
/// </para>
/// <para>
/// <b><see cref="TenantId"/> is nullable — shared plus extensible</b> (ADR-031's
/// second filter shape). A null-tenant playbook is the platform's and every tenant
/// can instantiate it; a tenant's own is theirs alone. Authoring the latter is
/// EPIC-012's, and the column is here now so that never needs a migration.
/// </para>
/// </remarks>
public sealed class ProcessDefinition : AggregateRoot<ProcessDefinitionId>
{
    public const int CodeMaxLength = 100;
    public const int NameMaxLength = 300;
    public const int DescriptionMaxLength = 4000;

    private readonly List<ProcessDefinitionVersion> _versions = [];

    // EF materialisation.
    private ProcessDefinition()
    {
    }

    /// <summary>
    /// null => the platform's, visible to every tenant.
    /// value => this tenant's own.
    /// </summary>
    public TenantId? TenantId { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public CountryId CountryId { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public ApplicationTypeId ApplicationTypeId { get; private set; }

    public ProcessDefinitionStatus Status { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public IReadOnlyCollection<ProcessDefinitionVersion> Versions
        => _versions.AsReadOnly();

    /// <summary>The version being written, if any. At most one.</summary>
    public ProcessDefinitionVersion? Draft
        => _versions.FirstOrDefault(
            x => x.Status == ProcessDefinitionVersionStatus.Draft);

    /// <summary>
    /// The version a new plan would be instantiated from — highest published
    /// version number wins.
    /// </summary>
    /// <remarks>
    /// Resolution by effective date is S003's problem, not this story's. What is
    /// settled here is that a <em>superseded</em> version is never a candidate,
    /// which is the whole point of the status.
    /// </remarks>
    public ProcessDefinitionVersion? CurrentVersion
        => _versions
            .Where(x => x.Status == ProcessDefinitionVersionStatus.Published)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault();

    public static ProcessDefinition Create(
        string code,
        string name,
        CountryId countryId,
        AuthorityId authorityId,
        ApplicationTypeId applicationTypeId,
        DateTime createdOnUtc,
        string? description = null,
        TenantId? tenantId = null)
        => Create(
            ProcessDefinitionId.New(),
            code,
            name,
            countryId,
            authorityId,
            applicationTypeId,
            createdOnUtc,
            description,
            tenantId);

    /// <summary>
    /// Deterministic-id overload, for seeding. Mirrors
    /// <c>RegulatoryTemplate.Create</c>: a seed that re-runs must not produce a
    /// second playbook.
    /// </summary>
    public static ProcessDefinition Create(
        ProcessDefinitionId id,
        string code,
        string name,
        CountryId countryId,
        AuthorityId authorityId,
        ApplicationTypeId applicationTypeId,
        DateTime createdOnUtc,
        string? description = null,
        TenantId? tenantId = null)
    {
        if (countryId == default)
            throw new DomainException(ProcessDefinitionErrors.CountryRequired);

        if (authorityId == default)
            throw new DomainException(ProcessDefinitionErrors.AuthorityRequired);

        if (applicationTypeId == default)
            throw new DomainException(
                ProcessDefinitionErrors.ApplicationTypeRequired);

        return new ProcessDefinition
        {
            Id = id,
            TenantId = tenantId,
            Code = Validated(
                code,
                CodeMaxLength,
                ProcessDefinitionErrors.CodeRequired,
                ProcessDefinitionErrors.CodeTooLong).ToUpperInvariant(),
            Name = Validated(
                name,
                NameMaxLength,
                ProcessDefinitionErrors.NameRequired,
                ProcessDefinitionErrors.NameTooLong),
            Description = OptionalDescription(description),
            CountryId = countryId,
            AuthorityId = authorityId,
            ApplicationTypeId = applicationTypeId,
            Status = ProcessDefinitionStatus.Active,
            CreatedOnUtc = createdOnUtc
        };
    }

    /// <summary>
    /// Opens the next draft version (N+1). The playbook owns numbering — a version
    /// number is never accepted from outside — and permits at most one open draft.
    /// </summary>
    public ProcessDefinitionVersion StartDraftVersion()
    {
        if (Draft is not null)
            throw new BusinessRuleViolationException(
                ProcessDefinitionErrors.DraftAlreadyOpen);

        // Max, not Count + 1: a discarded draft's number is reissued; a number
        // some plan was scheduled from never is.
        var nextNumber = _versions.Count == 0
            ? 1
            : _versions.Max(x => x.VersionNumber) + 1;

        var version = new ProcessDefinitionVersion(
            ProcessDefinitionVersionId.New(),
            nextNumber);

        _versions.Add(version);

        return version;
    }

    /// <summary>
    /// Freezes a version and makes it instantiable. <b>One-way</b> — ADR-065 I4.
    /// </summary>
    public void PublishVersion(
        ProcessDefinitionVersionId versionId,
        DateOnly? effectiveFrom,
        DateTime publishedOnUtc)
        => VersionOf(versionId).Publish(effectiveFrom, publishedOnUtc);

    /// <summary>
    /// Marks a published version superseded. <b>Nothing new instantiates from it;
    /// every plan already pinned to it keeps working, unmigrated.</b>
    /// </summary>
    public void SupersedeVersion(ProcessDefinitionVersionId versionId)
        => VersionOf(versionId).Supersede();

    /// <summary>Adds a step to the open draft. A published version is frozen.</summary>
    public ProcessStepDefinition AddStep(
        string code,
        string name,
        string? description = null,
        ProcessStepDefinitionId? parentStepId = null,
        int order = 0,
        int offsetDays = 0,
        int durationDays = 1)
        => OpenDraft().AddStep(
            code, name, description, parentStepId, order, offsetDays, durationDays);

    /// <summary>
    /// Records that one step of the open draft waits for another. Both must belong
    /// to that draft.
    /// </summary>
    public void AddStepPredecessor(
        ProcessStepDefinitionId stepId,
        ProcessStepDefinitionId predecessorStepId)
        => OpenDraft().AddPredecessor(stepId, predecessorStepId);

    /// <summary>Throws away the open draft. Only ever a draft (ADR-065 I4).</summary>
    public void DiscardDraft()
    {
        var draft = Draft
            ?? throw new NotFoundException(ProcessDefinitionErrors.NoOpenDraft);

        _versions.Remove(draft);
    }

    /// <summary>Lifecycle over deletion (ES-018). Published versions stay readable.</summary>
    public void Retire() => Status = ProcessDefinitionStatus.Retired;

    public void Reinstate() => Status = ProcessDefinitionStatus.Active;

    public void Rename(string name, string? description)
    {
        Name = Validated(
            name,
            NameMaxLength,
            ProcessDefinitionErrors.NameRequired,
            ProcessDefinitionErrors.NameTooLong);
        Description = OptionalDescription(description);
    }

    private ProcessDefinitionVersion OpenDraft()
        => Draft
           ?? throw new BusinessRuleViolationException(
               ProcessDefinitionErrors.NoOpenDraft);

    private ProcessDefinitionVersion VersionOf(ProcessDefinitionVersionId versionId)
        => _versions.FirstOrDefault(x => x.Id == versionId)
           ?? throw new NotFoundException(ProcessDefinitionErrors.VersionNotFound);

    private static string Validated(
        string value, int maxLength, string required, string tooLong)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(required);

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new DomainException(tooLong);

        return trimmed;
    }

    private static string? OptionalDescription(string? description)
        => string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
