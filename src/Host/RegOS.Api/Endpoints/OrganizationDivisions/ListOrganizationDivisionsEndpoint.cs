using RegOS.Organization.Application.Queries.Divisions.ListOrganizationDivisions;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.OrganizationDivisions;

public static class ListOrganizationDivisionsEndpoint
{
    public static IEndpointRouteBuilder MapListOrganizationDivisions(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizations/{organizationId:guid}/divisions", HandleAsync)
            .WithName("ListOrganizationDivisions")
            .WithSummary("The business units within one organization")
            .WithTags("Organization Divisions");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        ListOrganizationDivisionsHandler handler,
        CancellationToken cancellationToken)
    {
        var divisions = await handler.HandleAsync(
            new ListOrganizationDivisionsQuery(new OrganizationId(organizationId)),
            cancellationToken);

        return divisions is null ? Results.NotFound() : Results.Ok(divisions);
    }
}
