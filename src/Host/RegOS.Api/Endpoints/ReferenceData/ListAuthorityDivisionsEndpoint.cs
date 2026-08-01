using RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorityDivisions;
using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListAuthorityDivisionsEndpoint
{
    public static IEndpointRouteBuilder MapListAuthorityDivisions(
        this IEndpointRouteBuilder app)
    {
        // Nested under the authority because a division has no meaning without
        // one, and the route says so.
        app.MapGet(
            "/api/master-data/authorities/{authorityId:guid}/divisions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid authorityId,
        ListAuthorityDivisionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListAuthorityDivisionsQuery(new AuthorityId(authorityId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
