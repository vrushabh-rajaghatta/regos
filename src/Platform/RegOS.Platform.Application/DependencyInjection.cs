using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Commands.ActivateUser;
using RegOS.Platform.Application.Commands.DeactivateUser;
using RegOS.Platform.Application.Commands.InviteUser;
using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Invitations;
using RegOS.Platform.Application.Commands.AcceptInvitation;
using RegOS.Platform.Application.Commands.ActivateTenant;
using RegOS.Platform.Application.Commands.CreateTenant;
using RegOS.Platform.Application.Commands.DeactivateTenant;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Commands.Logout;
using RegOS.Platform.Application.Commands.RefreshSession;
using RegOS.Platform.Application.Commands.ChangePassword;
using RegOS.Platform.Application.Commands.CompletePasswordReset;
using RegOS.Platform.Application.Commands.RequestPasswordReset;
using RegOS.Platform.Application.Commands.RenameTenant;
using RegOS.Platform.Application.Commands.ResendInvitation;
using RegOS.Platform.Application.PasswordResets;
using RegOS.Platform.Application.Commands.SetUserPassword;
using RegOS.Platform.Application.Commands.UpdateUserProfile;
using RegOS.Platform.Application.Commands.RevokeSession;
using RegOS.Platform.Application.Queries.GetSessions;
using RegOS.Platform.Application.Queries.GetTenants;
using RegOS.Platform.Application.Queries.GetTenantUsers;
using RegOS.Platform.Application.Queries.GetUserById;
using RegOS.Platform.Application.Queries.GetUsers;

namespace RegOS.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(
        this IServiceCollection services)
    {
        services.AddScoped<InvitationIssuer>();

        services.AddScoped<InviteUserHandler>();

        services.AddScoped<ResendInvitationHandler>();

        services.AddScoped<AcceptInvitationHandler>();

        services.AddScoped<ActivateUserHandler>();

        services.AddScoped<DeactivateUserHandler>();

        services.AddScoped<UpdateUserProfileHandler>();

        services.AddScoped<SetUserPasswordHandler>();

        services.AddScoped<PasswordResetIssuer>();

        services.AddScoped<RequestPasswordResetHandler>();

        services.AddScoped<CompletePasswordResetHandler>();

        // Stateless: it composes two issuers and holds nothing per request.
        services.AddSingleton<SessionFactory>();

        // Scoped, unlike SessionFactory: these hold repositories.
        services.AddScoped<SessionRevoker>();

        services.AddScoped<CredentialTrustRevoker>();

        services.AddScoped<ChangePasswordHandler>();

        services.AddScoped<GetSessionsHandler>();

        services.AddScoped<RevokeSessionHandler>();

        services.AddScoped<LoginHandler>();

        services.AddScoped<RefreshSessionHandler>();

        services.AddScoped<LogoutHandler>();

        services.AddScoped<CreateTenantHandler>();

        services.AddScoped<RenameTenantHandler>();

        services.AddScoped<ActivateTenantHandler>();

        services.AddScoped<DeactivateTenantHandler>();

        services.AddScoped<GetTenantsHandler>();

        services.AddScoped<GetTenantUsersHandler>();

        services.AddScoped<GetUsersHandler>();

        services.AddScoped<GetUserByIdHandler>();

        return services;
    }
}
