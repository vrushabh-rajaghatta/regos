using Microsoft.Extensions.DependencyInjection;

using RegOS.Process.Application.Commands.ChangeProcessObjectiveStatus;
using RegOS.Process.Application.Commands.ChangeProcessPlanStatus;
using RegOS.Process.Application.Commands.ChangeProcessStepStatus;
using RegOS.Process.Application.Commands.ConfirmObjectiveMarketRecord;
using RegOS.Process.Application.Commands.CreateProcessObjective;
using RegOS.Process.Application.Commands.InstantiateProcessPlan;
using RegOS.Process.Application.Queries.GetProcessDefinition;
using RegOS.Process.Application.Queries.GetProcessObjective;
using RegOS.Process.Application.Queries.GetPlanImpact;
using RegOS.Process.Application.Queries.GetProcessPlan;
using RegOS.Process.Application.Queries.ListNextSteps;
using RegOS.Process.Application.Queries.ListObjectivePlans;
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

        services.AddScoped<InstantiateProcessPlanHandler>();

        services.AddScoped<GetProcessPlanHandler>();

        services.AddScoped<ListObjectivePlansHandler>();

        services.AddScoped<ChangeProcessPlanStatusHandler>();

        services.AddScoped<ChangeProcessStepStatusHandler>();

        services.AddScoped<ListNextStepsHandler>();

        services.AddScoped<GetPlanImpactHandler>();

        return services;
    }
}
