using RegOS.Api.Authentication;
using RegOS.Platform.Application.Commands.ChangePassword;

namespace RegOS.Api.Endpoints.Authentication;

public static class ChangePasswordEndpoint
{
    public static IEndpointRouteBuilder MapChangePassword(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/auth/change-password",
            HandleAsync)
        // The only credential flow that requires a session. The other two exist
        // precisely because the caller has none.
        .RequireAuthorization()
        .WithName("ChangePassword")
        .WithSummary("Replace your own password, proving you know the current one")
        .WithTags("Authentication");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ChangePasswordRequest request,
        ChangePasswordHandler handler,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangePasswordCommand(
                request.CurrentPassword, request.NewPassword),
            cancellationToken);

        // The handler has just revoked every refresh token, including this
        // caller's. Without this the browser would keep an access cookie that
        // works for the rest of its fifteen minutes while the refresh behind it
        // is dead - a half-signed-in state that reads as a bug. Signing back in
        // is the intended next step.
        SessionCookies.Clear(response);

        return Results.NoContent();
    }
}
