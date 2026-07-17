using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.ReferenceData.Domain.DocumentType;

public sealed class DocumentType
{
    private DocumentType()
    {
    }

    public DocumentTypeId Id { get; private set; }

    // null  => platform-provided system document type.
    // value => organization-specific extension.
    public OrganizationId? OrganizationId { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public static DocumentType Create(
        DocumentTypeId id,
        OrganizationId? organizationId,
        string code,
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                DocumentTypeErrors.CodeRequired,
                nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                DocumentTypeErrors.NameRequired,
                nameof(name));

        return new DocumentType
        {
            Id = id,
            OrganizationId = organizationId,
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
        => Create(id, organizationId: null, code, name, description);
}
