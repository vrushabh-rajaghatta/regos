using Microsoft.Extensions.DependencyInjection;

using RegOS.Process.Application.Commands.ChangeProcessObjectiveStatus;
using RegOS.Process.Application.Commands.ConfirmObjectiveMarketRecord;
using RegOS.Process.Application.Commands.CreateProcessObjective;
using RegOS.Process.Application.Queries.GetProcessDefinition;
using RegOS.Process.Application.Queries.GetProcessObjective;
using RegOS.Process.Application.Queries.ListProcessDefinitions;
using RegOS.Process.Application.Queries.ListProcessObjectives;

namespace RegOS.Process.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProcessApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListProcessDefinitionsHandler>();

        services.AddScoped<GetProcessDefinitionHandler>();

        services.AddScoped<CreateProcessObjectiveHandler>();

        services.AddScoped<ChangeProcessObjectiveStatusHandler>();

        services.AddScoped<ConfirmObjectiveMarketRecordHandler>();

        services.AddScoped<ListProcessObjectivesHandler>();

        services.AddScoped<GetProcessObjectiveHandler>();

        return services;
    }
}
