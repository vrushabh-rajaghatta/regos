using Microsoft.Extensions.DependencyInjection;
using RegOS.RegulatoryApplication.Application.Commands.RegisterApplication;

namespace RegOS.RegulatoryApplication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterApplicationHandler>();

        return services;
    }
}