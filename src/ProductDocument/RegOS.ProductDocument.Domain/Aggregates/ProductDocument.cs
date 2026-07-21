using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.Enums;
using RegOS.ProductDocument.Domain.Errors;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ProductDocument.Domain.Aggregates;

public sealed class ProductDocument
{
    public const int NameMaxLength = 200;

    private readonly List<DocumentVersion> _versions = [];

    private ProductDocument(
        ProductDocumentId id,
        TenantId tenantId,
        ProductId productId,
        DocumentTypeId documentTypeId,
        string name,
        DateTime createdOnUtc)
    {
        Id = id;
        TenantId = tenantId;
        ProductId = productId;
        DocumentTypeId = documentTypeId;
        Name = name;
        Status = ProductDocumentStatus.Draft;
        CurrentVersionId = null;
        CreatedOnUtc = createdOnUtc;
    }

    public ProductDocumentId Id { get; }

    // The owning tenant, derived from the owning product at upload so the
    // document can never carry a different tenant than its product (ADR-031).
    public TenantId TenantId { get; }

    public ProductId ProductId { get; }

    public DocumentTypeId DocumentTypeId { get; }

    public string Name { get; private set; }

    public ProductDocumentStatus Status { get; private set; }

    public DocumentVersionId? CurrentVersionId { get; private set; }

    public DateTime CreatedOnUtc { get; }

    // Never expose a mutable collection — version management stays inside
    // the aggregate.
    public IReadOnlyCollection<DocumentVersion> Versions
        => _versions.AsReadOnly();

    public static ProductDocument Create(
        TenantId tenantId,
        ProductId productId,
        DocumentTypeId documentTypeId,
        string name)
    {
        if (tenantId is null)
            throw new DomainException(ProductDocumentErrors.TenantRequired);

        if (productId == default)
            throw new DomainException(ProductDocumentErrors.ProductRequired);

        if (documentTypeId == default)
            throw new DomainException(ProductDocumentErrors.DocumentTypeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductDocumentErrors.DocumentNameRequired);

        var trimmedName = name.Trim();

        if (trimmedName.Length > NameMaxLength)
            throw new DomainException(ProductDocumentErrors.DocumentNameTooLong);

        return new ProductDocument(
            ProductDocumentId.New(),
            tenantId,
            productId,
            documentTypeId,
            trimmedName,
            DateTime.UtcNow);
    }

    /// <summary>Creates Version 1. Valid only when no versions exist.</summary>
    public void AddInitialVersion(
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum)
    {
        if (_versions.Count > 0)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.DocumentAlreadyHasInitialVersion);

        AppendVersion(
            1,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            storagePath,
            checksum);
    }

    /// <summary>
    /// Appends the next version (N+1). The aggregate owns numbering — a
    /// version number is never accepted from the outside. Not exposed as a
    /// user workflow in Sprint 18, but modelled so the invariant is correct.
    /// </summary>
    public void AddNewVersion(
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum)
    {
        if (_versions.Count == 0)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.DocumentHasNoInitialVersion);

        var nextVersionNumber = _versions.Max(v => v.VersionNumber) + 1;

        AppendVersion(
            nextVersionNumber,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            storagePath,
            checksum);
    }

    /// <summary>Draft -> Active. Requires a current version.</summary>
    public void Activate()
    {
        if (Status == ProductDocumentStatus.Archived)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.DocumentArchived);

        if (Status == ProductDocumentStatus.Active)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.DocumentAlreadyActive);

        // A document with no content is not something the business can
        // approve — the aggregate enforces this rather than trusting the
        // upload workflow to always have run first.
        if (CurrentVersionId is null)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.CannotActivateWithoutVersion);

        Status = ProductDocumentStatus.Active;
    }

    /// <summary>Active -> Archived. Archived is terminal.</summary>
    public void Archive()
    {
        if (Status == ProductDocumentStatus.Archived)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.DocumentArchived);

        if (Status == ProductDocumentStatus.Draft)
            throw new BusinessRuleViolationException(
                ProductDocumentErrors.CannotArchiveDraft);

        Status = ProductDocumentStatus.Archived;
    }

    private void AppendVersion(
        int versionNumber,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum)
    {
        var version = new DocumentVersion(
            DocumentVersionId.New(),
            versionNumber,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            storagePath,
            checksum,
            DateTime.UtcNow);

        _versions.Add(version);
        CurrentVersionId = version.Id;
    }
}
