using RegOS.Api.Authentication;
using RegOS.Platform.Application.Queries.GetUserById;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Api.Endpoints.Platform;

public static class GetUserEndpoint
{
    public static IEndpointRouteBuilder MapGetUser(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/platform/users/{id:guid}",
            HandleAsync)
        .WithName("GetUser")
        .WithSummary("Get user details")
        .WithTags("Platform")
        // User administration belongs to the tenant administrator
        // (ADR-033): a Member is refused with 403, and a platform
        // administrator has no tenant to administer users in.
        .RequireAuthorization(RegOSPolicies.TenantAdministrator);

        return app;
    }

    // The tenant is ambient, and now comes from the caller's token, so it is
    // not a parameter here and cannot be chosen by the caller at all. A
    // missing user and one outside the caller's tenant both surface as 404 via
    // the NotFoundException mapping, so the API never reveals that a record
    // exists in another organization.
    private static async Task<IResult> HandleAsync(
        Guid id,
        GetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetUserByIdQuery(
                UserId.From(id)),
            cancellationToken);

        return Results.Ok(result);
    }
}
