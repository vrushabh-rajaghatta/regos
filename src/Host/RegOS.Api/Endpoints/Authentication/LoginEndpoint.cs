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
        .WithName("Login")
        .WithSummary("Exchange an email and password for an access token")
        .WithTags("Authentication");

        return app;
    }

    // No try/catch: the handler raises AuthenticationFailedException and the
    // middleware maps it to 401 (ADR-022). No tenant header either — sign-in is
    // what establishes the tenant, so it cannot require one.
    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Results.Ok(
            new LoginResponse(result.AccessToken, result.ExpiresAt));
    }
}
