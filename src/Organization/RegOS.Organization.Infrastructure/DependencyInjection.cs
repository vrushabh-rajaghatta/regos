using Microsoft.Extensions.DependencyInjection;

namespace RegOS.Organization.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationInfrastructure(
        this IServiceCollection services)
    {
        return services;
    }
}
