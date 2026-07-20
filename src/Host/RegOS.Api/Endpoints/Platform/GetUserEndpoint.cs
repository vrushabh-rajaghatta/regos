using RegOS.Organization.Domain.Aggregates.Organization;
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
        .WithTags("Platform");

        return app;
    }

    // organizationId travels explicitly, the same way it does for inviting and
    // listing users — there is no authenticated tenant context to read it from
    // yet. A missing user and one outside the organization both surface as 404
    // via the NotFoundException mapping.
    private static async Task<IResult> HandleAsync(
        Guid id,
        GetUserByIdHandler handler,
        CancellationToken cancellationToken,
        Guid? organizationId = null)
    {
        var result = await handler.HandleAsync(
            new GetUserByIdQuery(
                UserId.From(id),
                organizationId is null
                    ? null
                    : new OrganizationId(organizationId.Value)),
            cancellationToken);

        return Results.Ok(result);
    }
}
