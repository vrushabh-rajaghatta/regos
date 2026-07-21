using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.RevokeSession;
using RegOS.Platform.Application.Queries.GetSessions;

namespace RegOS.Api.Endpoints.Authentication;

public static class SessionsEndpoints
{
    public static IEndpointRouteBuilder MapSessions(
        this IEndpointRouteBuilder app)
    {
        // Under /api/auth on purpose: the refresh cookie is scoped to that
        // path, and these endpoints need it to tell which session is the
        // caller's own. Anywhere else and the browser would not send it.
        app.MapGet("/api/auth/sessions", ListAsync)
            .RequireAuthorization()
            .WithName("ListSessions")
            .WithSummary("The signed-in user's own active sessions")
            .WithTags("Authentication");

        app.MapDelete("/api/auth/sessions/{sessionId:guid}", RevokeAsync)
            .RequireAuthorization()
            .WithName("RevokeSession")
            .WithSummary("End one of your own sessions")
            .WithTags("Authentication");

        app.MapPost("/api/auth/sessions/revoke-others", RevokeOthersAsync)
            .RequireAuthorization()
            .WithName("RevokeOtherSessions")
            .WithSummary("End every session except the one making the request")
            .WithTags("Authentication");

        return app;
    }

    private static async Task<IResult> ListAsync(
        GetSessionsHandler handler,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        request.Cookies.TryGetValue(SessionCookies.RefreshToken, out var refresh);

        return Results.Ok(await handler.HandleAsync(
            new GetSessionsQuery(refresh), cancellationToken));
    }

    private static async Task<IResult> RevokeAsync(
        Guid sessionId,
        RevokeSessionHandler handler,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        request.Cookies.TryGetValue(SessionCookies.RefreshToken, out var refresh);

        var wasCurrent = await handler.HandleAsync(
            new RevokeSessionCommand(sessionId, refresh), cancellationToken);

        // Revoking your own current session is signing yourself out, so the
        // cookies must go with it - otherwise the browser keeps presenting a
        // session the server has already ended.
        if (wasCurrent) SessionCookies.Clear(response);

        return Results.NoContent();
    }

    private static async Task<IResult> RevokeOthersAsync(
        RevokeSessionHandler handler,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        request.Cookies.TryGetValue(SessionCookies.RefreshToken, out var refresh);

        // No cookie clearing: the whole point is that this session survives.
        await handler.HandleAsync(
            new RevokeSessionCommand(SessionId: null, refresh),
            cancellationToken);

        return Results.NoContent();
    }
}
