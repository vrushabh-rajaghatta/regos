using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Organization.Application.Commands.CreateOrganizationSite;

public sealed class CreateOrganizationSiteHandler
{
    private readonly IOrganizationSiteCreationPolicy _policy;
    private readonly IOrganizationSiteRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateOrganizationSiteHandler(
        IOrganizationSiteCreationPolicy policy,
        IOrganizationSiteRepository repository,
        ITenantContext tenantContext)
    {
        _policy = policy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<OrganizationSiteId> HandleAsync(
        CreateOrganizationSiteCommand command,
        CancellationToken cancellationToken)
    {
        var identifiers = command.Identifiers ?? [];

        // Everything that depends on other tables, checked first.
        await _policy.EnsureCanCreateAsync(
            command.OrganizationId,
            command.CountryId,
            [.. identifiers.Select(x => x.SchemeId)],
            cancellationToken);

        var address = PostalAddress.Create(
            command.CountryId,
            command.AddressLine1,
            command.AddressLine2,
            command.AddressLine3,
            command.City,
            command.StateProvince,
            command.PostalCode);

        var site = OrganizationSite.Create(
            _tenantContext.TenantId,
            command.OrganizationId,
            command.Name,
            command.Type,
            address,
            command.StatusDate,
            command.NameNativeLanguage,
            command.Email,
            command.Phone);

        // One per scheme is the aggregate's rule, so a duplicated scheme in the
        // request is refused here rather than silently collapsed.
        foreach (var identifier in identifiers)
            site.AddIdentifier(identifier.SchemeId, identifier.Value);

        await _repository.AddAsync(site, cancellationToken);

        return site.Id;
    }
}
