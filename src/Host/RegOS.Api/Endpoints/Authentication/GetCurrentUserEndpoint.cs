using RegOS.Platform.Application.Services;
using RegOS.SharedKernel.Abstractions;
using RegOS.Platform.Contracts;

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
    // point of the slice. The tenant is read through the lenient
    // ITenantContext accessor rather than ICurrentUser.TenantId, which throws
    // for a platform user — whose /me legitimately has no tenant to report.
    private static IResult Handle(
        ICurrentUser currentUser,
        ITenantContext tenantContext) =>
        Results.Ok(new CurrentUserResponse(
            currentUser.UserId.Value,
            tenantContext.TenantIdOrNull?.Value,
            currentUser.Email.Value,
            currentUser.Role.ToString()));
}
