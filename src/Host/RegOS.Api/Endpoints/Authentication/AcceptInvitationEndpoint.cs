using RegOS.Platform.Application.Commands.AcceptInvitation;

namespace RegOS.Api.Endpoints.Authentication;

public static class AcceptInvitationEndpoint
{
    public static IEndpointRouteBuilder MapAcceptInvitation(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/invitations/accept",
            HandleAsync)
        // Anonymous by definition: the caller has no session, and obtaining the
        // ability to have one is the point. The token is the credential.
        .AllowAnonymous()
        .WithName("AcceptInvitation")
        .WithSummary("Set a first password and activate an invited account")
        .WithTags("Authentication");

        return app;
    }

    // No cookies are set. Accepting proves you hold the invitation, not that
    // you know the password you have just chosen - so the user signs in
    // afterwards like anyone else, through the one path that issues sessions.
    private static async Task<IResult> HandleAsync(
        AcceptInvitationRequest request,
        AcceptInvitationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AcceptInvitationCommand(request.Token, request.Password),
            cancellationToken);

        return Results.NoContent();
    }
}
