using Microsoft.Extensions.DependencyInjection;
using RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;
using RegOS.RegulatoryApplication.Application.Queries.GetRegulatoryApplication;
using RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

namespace RegOS.RegulatoryApplication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRegulatoryApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<CreateRegulatoryApplicationHandler>();

        services.AddScoped<ListRegulatoryApplicationsHandler>();

        services.AddScoped<GetRegulatoryApplicationHandler>();

        return services;
    }
}
