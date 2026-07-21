using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.ActivateUser;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Api.Endpoints.Platform;

public static class ActivateUserEndpoint
{
    public static IEndpointRouteBuilder MapActivateUser(
        this IEndpointRouteBuilder app)
    {
        // An action endpoint, not PUT /status: activation is a business
        // operation rather than a generic property update.
        app.MapPost(
            "/api/platform/users/{id:guid}/activate",
            HandleAsync)
        .WithName("ActivateUser")
        .WithSummary("Activate a user")
        .WithTags("Platform")
        // User administration belongs to the tenant administrator
        // (ADR-033): a Member is refused with 403, and a platform
        // administrator has no tenant to administer users in.
        .RequireAuthorization(RegOSPolicies.TenantAdministrator);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ActivateUserHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ActivateUserCommand(
                UserId.From(id)),
            cancellationToken);

        return Results.NoContent();
    }
}
