using Microsoft.Extensions.DependencyInjection;

using RegOS.Organization.Application.Commands.CreateOrganization;
using RegOS.Organization.Application.Commands.DeactivateOrganization;
using RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

namespace RegOS.Organization.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateOrganizationHandler>();
        services.AddScoped<DeactivateOrganizationHandler>();
        services.AddScoped<ListOrganizationsHandler>();

        return services;
    }
}
