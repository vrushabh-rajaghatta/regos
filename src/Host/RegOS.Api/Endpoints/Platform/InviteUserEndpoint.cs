using RegOS.Platform.Application.Commands.InviteUser;

namespace RegOS.Api.Endpoints.Platform;

public static class InviteUserEndpoint
{
    public static IEndpointRouteBuilder MapInviteUser(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/platform/users/invitations",
            HandleAsync);

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
