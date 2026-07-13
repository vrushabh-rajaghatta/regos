using Microsoft.Extensions.DependencyInjection;

using RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

namespace RegOS.Organization.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListOrganizationsHandler>();

        return services;
    }
}
