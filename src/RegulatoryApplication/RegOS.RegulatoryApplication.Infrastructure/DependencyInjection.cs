using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.RegulatoryApplication.Application.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.RegulatoryApplication.Infrastructure.Repositories;
using RegOS.RegulatoryApplication.Infrastructure.Services;

namespace RegOS.RegulatoryApplication.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRegulatoryApplicationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRegulatoryApplicationRepository,
            RegulatoryApplicationRepository>();

        services.AddScoped<IRegulatoryApplicationCreationPolicy,
            RegulatoryApplicationCreationPolicy>();

        return services;
    }
}
