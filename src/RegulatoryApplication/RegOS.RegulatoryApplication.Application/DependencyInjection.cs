using Microsoft.Extensions.DependencyInjection;
using RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRegulatoryApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<CreateRegulatoryApplicationHandler>();

        return services;
    }
}
