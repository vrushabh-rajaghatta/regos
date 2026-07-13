using Microsoft.Extensions.DependencyInjection;

namespace RegOS.MasterData.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMasterDataInfrastructure(
        this IServiceCollection services)
    {
        return services;
    }
}
