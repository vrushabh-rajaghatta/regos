using Microsoft.Extensions.DependencyInjection;

using RegOS.MasterData.Application.Queries.Geography.ListCountries;
using RegOS.MasterData.Application.Queries.Regulatory.ListAuthorities;

namespace RegOS.MasterData.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMasterDataApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListCountriesHandler>();
        services.AddScoped<ListAuthoritiesHandler>();

        return services;
    }
}
