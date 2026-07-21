using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.Login;

namespace RegOS.Api.Endpoints.Authentication;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLogin(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/login",
            HandleAsync)
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Exchange an email and password for a session")
        .WithTags("Authentication");

        return app;
    }

    // No try/catch: the handler raises AuthenticationFailedException and the
    // middleware maps it to 401 (ADR-022). No tenant header either — sign-in is
    // what establishes the tenant, so it cannot require one.
    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        LoginHandler handler,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var session = await handler.HandleAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        SessionCookies.Write(response, session);

        // 204, not the tokens. The response body used to carry the access token
        // for JavaScript to store; the whole point of AUTH-006 is that it no
        // longer does. Returning it "just for convenience" would put it back
        // within reach of any script on the page.
        return Results.NoContent();
    }
}
