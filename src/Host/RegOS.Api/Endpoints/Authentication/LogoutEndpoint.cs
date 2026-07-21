using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.Logout;

namespace RegOS.Api.Endpoints.Authentication;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogout(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/logout",
            HandleAsync)
        // Anonymous, so signing out works even once the access token has
        // expired. Requiring authentication would mean the one action a user
        // takes when something has gone wrong is the action that fails.
        .AllowAnonymous()
        .WithName("Logout")
        .WithSummary("End the current session")
        .WithTags("Authentication");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        LogoutHandler handler,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        request.Cookies.TryGetValue(
            SessionCookies.RefreshToken, out var refreshToken);

        await handler.HandleAsync(
            new LogoutCommand(refreshToken), cancellationToken);

        // Cleared unconditionally, even when there was no token to revoke: the
        // caller asked to be signed out, and they are.
        SessionCookies.Clear(response);

        return Results.NoContent();
    }
}
