using RegOS.Platform.Application.Commands.RequestPasswordReset;

namespace RegOS.Api.Endpoints.Authentication;

public static class RequestPasswordResetEndpoint
{
    public static IEndpointRouteBuilder MapRequestPasswordReset(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/password-reset/request",
            HandleAsync)
        // Anonymous by definition: someone who has forgotten their password
        // cannot sign in to ask for a new one.
        .AllowAnonymous()
        .WithName("RequestPasswordReset")
        .WithSummary("Send a password reset link, if the address belongs to an active account")
        .WithTags("Authentication");

        return app;
    }

    // Always 204, whatever happened. The handler decides in silence whether
    // there was anybody to write to; saying so here would turn this into an
    // account-enumeration oracle for any address a stranger cares to try.
    private static async Task<IResult> HandleAsync(
        RequestPasswordResetRequest request,
        RequestPasswordResetHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RequestPasswordResetCommand(request.Email),
            cancellationToken);

        return Results.NoContent();
    }
}
