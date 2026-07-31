using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.ReferenceData.Domain.Organization;

/// <summary>
/// What a named person does in regulatory work — Qualified Person, Authorised
/// Representative, Regulatory Contact.
/// </summary>
/// <remarks>
/// <b>Shared plus extensible</b>, unlike <see cref="IdentifierScheme"/>, and the
/// difference is ownership. An identifier scheme describes the outside world: a
/// DUNS number does not differ by tenant. A role describes how a company chooses
/// to organise people, and the vocabulary is genuinely mixed —
/// <em>Qualified Person</em> and <em>Authorised Representative</em> are defined
/// by legislation, while <em>APAC Regulatory Lead</em> is one company's own
/// word. RegOS ships the first kind and lets a tenant add the second.
/// <para>
/// A null <see cref="TenantId"/> means a role the platform ships, visible to
/// everyone. A set one means a tenant's own, visible only to them — the same
/// shape as <c>DocumentType</c> and <c>RegulatoryTemplate</c>.
/// </para>
/// </remarks>
public sealed class ContactRole
{
    private ContactRole()
    {
    }

    public ContactRoleId Id { get; private set; }

    /// <summary>Null for a platform role; set for a tenant's own.</summary>
    public TenantId? TenantId { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public static ContactRole Create(
        ContactRoleId id,
        string code,
        string name,
        string? description = null,
        TenantId? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(ContactRoleErrors.CodeRequired);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ContactRoleErrors.NameRequired);

        return new ContactRole
        {
            Id = id,
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
        };
    }
}

public static class ContactRoleErrors
{
    public const string CodeRequired = "A contact role needs a code.";

    public const string NameRequired = "A contact role needs a name.";
}
