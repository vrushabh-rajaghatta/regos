using Microsoft.Extensions.DependencyInjection;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Infrastructure.Repositories;

namespace RegOS.Process.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProcessInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IProcessDefinitionRepository, ProcessDefinitionRepository>();

        return services;
    }
}
