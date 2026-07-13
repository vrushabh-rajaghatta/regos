using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.Persistence.Initialization;
using RegOS.Persistence.Initialization.MasterData;

namespace RegOS.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RegOSDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("RegOS")));

        services.AddScoped<IDataInitializer, MasterDataInitializer>();

        return services;
    }
}
