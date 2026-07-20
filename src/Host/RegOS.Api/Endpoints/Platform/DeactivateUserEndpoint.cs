using RegOS.Platform.Application.Commands.DeactivateUser;
using RegOS.Platform.Domain.Aggregates.User;

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
        .WithTags("Platform");

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
