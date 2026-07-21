using Microsoft.Extensions.DependencyInjection;

using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Infrastructure.Persistence;

namespace RegOS.Organization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        return services;
    }
}
