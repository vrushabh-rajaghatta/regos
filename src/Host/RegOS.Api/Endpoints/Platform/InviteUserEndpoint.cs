using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.InviteUser;

namespace RegOS.Api.Endpoints.Platform;

public static class InviteUserEndpoint
{
    public static IEndpointRouteBuilder MapInviteUser(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/platform/users/invitations",
            HandleAsync)
        // User administration belongs to the tenant administrator
        // (ADR-033): a Member is refused with 403, and a platform
        // administrator has no tenant to administer users in.
        .RequireAuthorization(RegOSPolicies.TenantAdministrator);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        InviteUserRequest request,
        InviteUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new InviteUserCommand(
                request.FirstName,
                request.LastName,
                request.Email),
            cancellationToken);

        return Results.Created(
            $"/api/platform/users/{result.Id.Value}",
            new InviteUserResponse(result.Id.Value, result.Status));
    }
}
