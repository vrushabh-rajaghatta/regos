using Microsoft.Extensions.DependencyInjection;

using RegOS.Interaction.Application.Commands.RecordCorrespondence;
using RegOS.Interaction.Application.Queries.GetCorrespondence;
using RegOS.Interaction.Application.Queries.ListCorrespondence;

namespace RegOS.Interaction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInteractionApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RecordCorrespondenceHandler>();

        services.AddScoped<ListCorrespondenceHandler>();

        services.AddScoped<GetCorrespondenceHandler>();

        return services;
    }
}
