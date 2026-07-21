using RegOS.Platform.Application.Services;

namespace RegOS.Api.Endpoints.Authentication;

public static class GetCurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentUser(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/auth/me",
            Handle)
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("The user behind the current access token")
        .WithTags("Authentication");

        return app;
    }

    // The first endpoint in RegOS that requires authentication. No tenant
    // header: identity and tenant both come from the token, which is the whole
    // point of the slice.
    private static IResult Handle(ICurrentUser currentUser) =>
        Results.Ok(new CurrentUserResponse(
            currentUser.UserId.Value,
            currentUser.OrganizationId.Value,
            currentUser.Email.Value));
}
