using Microsoft.Extensions.DependencyInjection;

using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationDivision;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Organization.Infrastructure.Services;
using RegOS.Organization.Infrastructure.Persistence;

namespace RegOS.Organization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        services.AddScoped<
            IOrganizationSiteRepository, OrganizationSiteRepository>();

        services.AddScoped<
            IOrganizationSiteCreationPolicy, OrganizationSiteCreationPolicy>();

        services.AddScoped<
            IOrganizationIdentifierPolicy, OrganizationIdentifierPolicy>();

        services.AddScoped<IContactRepository, ContactRepository>();

        services.AddScoped<IContactCreationPolicy, ContactCreationPolicy>();

        services.AddScoped<
            IOrganizationDivisionRepository, OrganizationDivisionRepository>();

        return services;
    }
}
