using Microsoft.Extensions.DependencyInjection;

using RegOS.Organization.Application.Commands.CreateOrganization;
using RegOS.Organization.Application.Commands.DeactivateOrganization;
using RegOS.Organization.Application.Commands.UpdateOrganization;
using RegOS.Organization.Application.Queries.Organizations.GetOrganization;
using RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

namespace RegOS.Organization.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateOrganizationHandler>();
        services.AddScoped<DeactivateOrganizationHandler>();
        services.AddScoped<GetOrganizationHandler>();
        services.AddScoped<ListOrganizationsHandler>();
        services.AddScoped<UpdateOrganizationHandler>();

        return services;
    }
}
