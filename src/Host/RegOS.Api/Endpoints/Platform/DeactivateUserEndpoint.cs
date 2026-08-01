using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.DeactivateUser;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Endpoints.Platform;

public static class DeactivateUserEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateUser(
        this IEndpointRouteBuilder app)
    {
        // A business action, not a generic property update - and deliberately
        // not a DELETE: deactivation preserves the user and their history.
        app.MapPost(
            "/api/platform/users/{id:guid}/deactivate",
            HandleAsync)
        .WithName("DeactivateUser")
        .WithSummary("Deactivate a user")
        .WithTags("Platform")
        // User administration belongs to the tenant administrator
        // (ADR-033): a Member is refused with 403, and a platform
        // administrator has no tenant to administer users in.
        .RequireAuthorization(RegOSPolicies.TenantAdministrator);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        DeactivateUserHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DeactivateUserCommand(
                UserId.From(id)),
            cancellationToken);

        return Results.NoContent();
    }
}
