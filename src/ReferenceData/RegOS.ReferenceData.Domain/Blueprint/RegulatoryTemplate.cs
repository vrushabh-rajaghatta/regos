using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// The identity of a dossier blueprint — "what a submission of this type, to
/// this authority, must contain" — independent of its versions. The aggregate
/// root: it owns its versions and assigns their sequential numbers.
/// </summary>
public sealed class RegulatoryTemplate
{
    public const int CodeMaxLength = 100;
    public const int NameMaxLength = 200;
    public const int SourceMaxLength = 200;

    private readonly List<RegulatoryTemplateVersion> _versions = [];

    private RegulatoryTemplate(
        RegulatoryTemplateId id,
        string code,
        string name,
        AuthorityId authorityId,
        SubmissionTypeId submissionTypeId,
        TenantId? tenantId,
        string source,
        DateTime createdOnUtc)
    {
        Id = id;
        Code = code;
        Name = name;
        AuthorityId = authorityId;
        SubmissionTypeId = submissionTypeId;
        TenantId = tenantId;
        Source = source;
        Status = RegulatoryTemplateStatus.Active;
        CreatedOnUtc = createdOnUtc;
    }

    public RegulatoryTemplateId Id { get; }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public AuthorityId AuthorityId { get; private set; }

    public SubmissionTypeId SubmissionTypeId { get; private set; }

    // null  => platform-shared blueprint, visible to every tenant.
    // value => tenant-owned (cloning arrives in a later epic; the column is
    //          here now so that never needs a migration).
    public TenantId? TenantId { get; private set; }

    // Provenance — the standard this blueprint derives from (e.g. "ICH eCTD").
    public string Source { get; private set; }

    public RegulatoryTemplateStatus Status { get; private set; }

    public DateTime CreatedOnUtc { get; }

    // Version management stays inside the aggregate — never expose a mutable
    // collection.
    public IReadOnlyCollection<RegulatoryTemplateVersion> Versions
        => _versions.AsReadOnly();

    public static RegulatoryTemplate Create(
        string code,
        string name,
        AuthorityId authorityId,
        SubmissionTypeId submissionTypeId,
        string source,
        TenantId? tenantId = null)
        => Create(
            RegulatoryTemplateId.New(),
            code,
            name,
            authorityId,
            submissionTypeId,
            source,
            tenantId);

    // Deterministic-id overload for seeding (mirrors Country/Authority.Create).
    public static RegulatoryTemplate Create(
        RegulatoryTemplateId id,
        string code,
        string name,
        AuthorityId authorityId,
        SubmissionTypeId submissionTypeId,
        string source,
        TenantId? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(RegulatoryTemplateErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(RegulatoryTemplateErrors.NameRequired);

        if (authorityId == default)
            throw new DomainException(RegulatoryTemplateErrors.AuthorityRequired);

        if (submissionTypeId == default)
            throw new DomainException(RegulatoryTemplateErrors.SubmissionTypeRequired);

        if (string.IsNullOrWhiteSpace(source))
            throw new DomainException(RegulatoryTemplateErrors.SourceRequired);

        return new RegulatoryTemplate(
            id,
            code.Trim().ToUpperInvariant(),
            name.Trim(),
            authorityId,
            submissionTypeId,
            tenantId,
            source.Trim(),
            DateTime.UtcNow);
    }

    /// <summary>
    /// Opens the next draft version (N+1). The aggregate owns numbering — a
    /// version number is never accepted from outside — and permits at most one
    /// open draft at a time.
    /// </summary>
    public RegulatoryTemplateVersion StartDraftVersion()
    {
        if (_versions.Any(v => v.Status == TemplateVersionStatus.Draft))
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.DraftAlreadyExists);

        var nextNumber = _versions.Count == 0
            ? 1
            : _versions.Max(v => v.VersionNumber) + 1;

        var version = new RegulatoryTemplateVersion(
            RegulatoryTemplateVersionId.New(),
            nextNumber);

        _versions.Add(version);

        return version;
    }

    /// <summary>Publishes (freezes) one of this template's versions.</summary>
    public void PublishVersion(
        RegulatoryTemplateVersionId versionId,
        DateOnly? effectiveFrom,
        DateTime publishedOnUtc)
    {
        var version = _versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.VersionNotFound);

        version.Publish(effectiveFrom, publishedOnUtc);
    }
}
