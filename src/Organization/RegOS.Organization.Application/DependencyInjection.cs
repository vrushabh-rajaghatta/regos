using Microsoft.Extensions.DependencyInjection;

using RegOS.Organization.Application.Commands.ActivateOrganization;
using RegOS.Organization.Application.Commands.CreateOrganization;
using RegOS.Organization.Application.Commands.CreateOrganizationSite;
using RegOS.Organization.Application.Commands.DeactivateOrganization;
using RegOS.Organization.Application.Commands.UpdateOrganization;
using RegOS.Organization.Application.Queries.Organizations.GetOrganization;
using RegOS.Organization.Application.Queries.Organizations.ListOrganizations;
using RegOS.Organization.Application.Queries.Sites.GetOrganizationSite;
using RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;
using RegOS.Organization.Application.Queries.Sites.SiteDirectory;

namespace RegOS.Organization.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ActivateOrganizationHandler>();
        services.AddScoped<CreateOrganizationHandler>();
        services.AddScoped<DeactivateOrganizationHandler>();
        services.AddScoped<GetOrganizationHandler>();
        services.AddScoped<ListOrganizationsHandler>();
        services.AddScoped<UpdateOrganizationHandler>();

        services.AddScoped<CreateOrganizationSiteHandler>();
        services.AddScoped<GetOrganizationSiteHandler>();
        services.AddScoped<ListOrganizationSitesHandler>();
        services.AddScoped<SiteDirectoryHandler>();

        return services;
    }
}
