using RegOS.Organization.Application.Queries.Organizations.GetOrganization;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.Organization;

public static class GetOrganizationEndpoint
{
    public static IEndpointRouteBuilder MapGetOrganization(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/organizations/{id:guid}",
            HandleAsync)
        .WithName("GetOrganization")
        .WithSummary("Get an organization")
        .WithTags("Organization");

        return app;
    }

    // No null check and no catch: the handler raises NotFoundException and the
    // middleware maps it to 404, the same as every other capability.
    private static async Task<IResult> HandleAsync(
        Guid id,
        GetOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new GetOrganizationQuery(new OrganizationId(id)),
            cancellationToken);

        return Results.Ok(response);
    }
}
