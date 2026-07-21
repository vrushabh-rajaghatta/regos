using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Commands.ActivateUser;
using RegOS.Platform.Application.Commands.DeactivateUser;
using RegOS.Platform.Application.Commands.InviteUser;
using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.Logout;
using RegOS.Platform.Application.Commands.RefreshSession;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Commands.UpdateUserProfile;
using RegOS.Platform.Application.Queries.GetUserById;
using RegOS.Platform.Application.Queries.GetUsers;

namespace RegOS.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(
        this IServiceCollection services)
    {
        services.AddScoped<InviteUserHandler>();

        services.AddScoped<ActivateUserHandler>();

        services.AddScoped<DeactivateUserHandler>();

        services.AddScoped<UpdateUserProfileHandler>();

        services.AddScoped<SetUserPasswordHandler>();

        // Stateless: it composes two issuers and holds nothing per request.
        services.AddSingleton<SessionFactory>();

        services.AddScoped<LoginHandler>();

        services.AddScoped<RefreshSessionHandler>();

        services.AddScoped<LogoutHandler>();

        services.AddScoped<GetUsersHandler>();

        services.AddScoped<GetUserByIdHandler>();

        return services;
    }
}
