using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.RegulatoryApplication.Infrastructure.Persistence;
using RegOS.RegulatoryApplication.Infrastructure.Repositories;

namespace RegOS.RegulatoryApplication.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRegulatoryApplicationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RegulatoryApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("RegOS")));

        services.AddScoped<IRegulatoryApplicationRepository,
            RegulatoryApplicationRepository>();

        return services;
    }
}
