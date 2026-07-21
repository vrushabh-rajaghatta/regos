using RegOS.Platform.Application.Commands.CompletePasswordReset;

namespace RegOS.Api.Endpoints.Authentication;

public static class CompletePasswordResetEndpoint
{
    public static IEndpointRouteBuilder MapCompletePasswordReset(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/password-reset/complete",
            HandleAsync)
        // Anonymous by definition: the token is the credential.
        .AllowAnonymous()
        .WithName("CompletePasswordReset")
        .WithSummary("Choose a new password using a reset link")
        .WithTags("Authentication");

        return app;
    }

    // No cookies are set. Holding a reset link proves control of a mailbox, not
    // knowledge of the password just chosen, so the user signs in afterwards
    // like anyone else - the same reasoning as accepting an invitation.
    private static async Task<IResult> HandleAsync(
        CompletePasswordResetRequest request,
        CompletePasswordResetHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new CompletePasswordResetCommand(request.Token, request.Password),
            cancellationToken);

        return Results.NoContent();
    }
}
