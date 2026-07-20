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
        .WithTags("Platform");

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
