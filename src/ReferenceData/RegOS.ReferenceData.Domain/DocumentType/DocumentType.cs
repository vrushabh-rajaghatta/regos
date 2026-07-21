using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.DocumentType;

public sealed class DocumentType
{
    private DocumentType()
    {
    }

    public DocumentTypeId Id { get; private set; }

    // null  => platform-provided system document type.
    // value => tenant-specific extension.
    public TenantId? TenantId { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public static DocumentType Create(
        DocumentTypeId id,
        TenantId? tenantId,
        string code,
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(DocumentTypeErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DocumentTypeErrors.NameRequired);

        return new DocumentType
        {
            Id = id,
            TenantId = tenantId,
            // Code is normalized and, having no setter or mutator, is
            // immutable after creation.
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            IsActive = true,
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    // Convenience factory for platform-provided system types (seeding).
    public static DocumentType CreateSystemType(
        DocumentTypeId id,
        string code,
        string name,
        string? description = null)
        => Create(id, tenantId: null, code, name, description);
}
