using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Commands.InviteUser;

namespace RegOS.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(
        this IServiceCollection services)
    {
        services.AddScoped<InviteUserHandler>();

        return services;
    }
}
