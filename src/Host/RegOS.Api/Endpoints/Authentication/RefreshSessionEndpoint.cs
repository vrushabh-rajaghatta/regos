using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.RefreshSession;

namespace RegOS.Api.Endpoints.Authentication;

public static class RefreshSessionEndpoint
{
    public static IEndpointRouteBuilder MapRefreshSession(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/refresh",
            HandleAsync)
        // Anonymous by necessity: the caller reaches here precisely because
        // their access token has expired. The refresh cookie is the credential,
        // and the handler treats it as one.
        .AllowAnonymous()
        .WithName("RefreshSession")
        .WithSummary("Exchange a refresh token for a new session")
        .WithTags("Authentication");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        RefreshSessionHandler handler,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        request.Cookies.TryGetValue(
            SessionCookies.RefreshToken, out var refreshToken);

        // There is deliberately no request body. Reading the token from a
        // parameter as well would give a caller a way to present one the
        // browser did not send.
        var session = await handler.HandleAsync(
            new RefreshSessionCommand(refreshToken),
            cancellationToken);

        SessionCookies.Write(response, session);

        return Results.NoContent();
    }
}
