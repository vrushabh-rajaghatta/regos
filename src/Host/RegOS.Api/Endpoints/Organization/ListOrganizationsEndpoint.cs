using RegOS.Organization.Application.Queries.Organizations.ListOrganizations;

namespace RegOS.Api.Endpoints.Organization;

public static class ListOrganizationsEndpoint
{
    public static IEndpointRouteBuilder MapListOrganizations(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/organizations",
            HandleAsync)
        .WithName("ListOrganizations")
        .WithSummary("List organizations")
        .WithTags("Organization");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListOrganizationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        return Results.Ok(result);
    }
}
