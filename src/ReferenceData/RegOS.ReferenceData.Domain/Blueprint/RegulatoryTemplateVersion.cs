using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// One version of a dossier blueprint. Created as a <see cref="TemplateVersionStatus.Draft"/>
/// by its owning <see cref="RegulatoryTemplate"/> and frozen on
/// <see cref="Publish"/>. Its sections, required documents and validation rules
/// (later stories) will hang off this version and inherit its immutability.
/// </summary>
public sealed class RegulatoryTemplateVersion
{
    // Internal so only the RegulatoryTemplate aggregate (same assembly) can
    // create a version — there is no path to instantiate one independently of
    // its root.
    internal RegulatoryTemplateVersion(
        RegulatoryTemplateVersionId id,
        int versionNumber)
    {
        Id = id;
        VersionNumber = versionNumber;
        Status = TemplateVersionStatus.Draft;
    }

    public RegulatoryTemplateVersionId Id { get; }

    public int VersionNumber { get; }

    public TemplateVersionStatus Status { get; private set; }

    // Temporal validity — the governance seam. Set at publish; the query
    // logic that uses it ("which version applied on date X") is deferred.
    public DateOnly? EffectiveFrom { get; private set; }

    public DateOnly? EffectiveTo { get; private set; }

    public DateTime? PublishedOnUtc { get; private set; }

    internal void Publish(DateOnly? effectiveFrom, DateTime publishedOnUtc)
    {
        if (Status == TemplateVersionStatus.Published)
            throw new BusinessRuleViolationException(
                RegulatoryTemplateErrors.VersionAlreadyPublished);

        Status = TemplateVersionStatus.Published;
        EffectiveFrom = effectiveFrom;
        PublishedOnUtc = publishedOnUtc;
    }
}
