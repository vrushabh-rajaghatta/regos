using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.SharedKernel.Abstractions;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;

namespace RegOS.Organization.Application.Commands.CreateContact;

public sealed class CreateContactHandler
{
    private readonly IContactCreationPolicy _policy;
    private readonly IContactRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateContactHandler(
        IContactCreationPolicy policy,
        IContactRepository repository,
        ITenantContext tenantContext)
    {
        _policy = policy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<ContactId> HandleAsync(
        CreateContactCommand command,
        CancellationToken cancellationToken)
    {
        var roleIds = command.RoleIds ?? [];

        await _policy.EnsureCanCreateAsync(
            command.OrganizationId,
            command.OrganizationSiteId,
            command.CountryId,
            roleIds,
            cancellationToken);

        var contact = ContactAggregate.Create(
            _tenantContext.TenantId,
            command.OrganizationId,
            command.FirstName,
            command.LastName,
            command.StatusDate,
            command.OrganizationSiteId,
            command.Title,
            command.Department,
            command.CountryId);

        // The aggregate owns the "no duplicates" rules; a repeated role or
        // address in the request is refused rather than silently collapsed.
        foreach (var roleId in roleIds)
            contact.AddRole(roleId);

        foreach (var email in command.Emails ?? [])
            contact.AddEmail(email);

        foreach (var phone in command.Phones ?? [])
            contact.AddPhone(phone.Number, phone.Kind);

        await _repository.AddAsync(contact, cancellationToken);

        return contact.Id;
    }
}
