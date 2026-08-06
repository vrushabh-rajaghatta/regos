using Microsoft.Extensions.DependencyInjection;

using RegOS.Process.Application.Queries.GetProcessDefinition;
using RegOS.Process.Application.Queries.ListProcessDefinitions;

namespace RegOS.Process.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProcessApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListProcessDefinitionsHandler>();

        services.AddScoped<GetProcessDefinitionHandler>();

        return services;
    }
}
