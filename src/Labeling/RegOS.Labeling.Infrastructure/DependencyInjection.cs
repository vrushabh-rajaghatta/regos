using Microsoft.Extensions.DependencyInjection;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Labeling.Infrastructure.Repositories;

namespace RegOS.Labeling.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLabelingInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IGlobalLabelRepository, GlobalLabelRepository>();

        services.AddScoped<ILocalLabelRepository, LocalLabelRepository>();

        services.AddScoped<IIndicationRepository, IndicationRepository>();

        return services;
    }
}
