using RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;
using RegOS.Organization.Domain.Aggregates.Organization;

namespace RegOS.Api.Endpoints.OrganizationSites;

public static class ListOrganizationSitesEndpoint
{
    public static IEndpointRouteBuilder MapListOrganizationSites(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizations/{organizationId:guid}/sites", HandleAsync)
            .WithName("ListOrganizationSites")
            .WithSummary("The sites one organization operates")
            .WithTags("Organization Sites");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        ListOrganizationSitesHandler handler,
        CancellationToken cancellationToken)
    {
        var sites = await handler.HandleAsync(
            new ListOrganizationSitesQuery(new OrganizationId(organizationId)),
            cancellationToken);

        return sites is null ? Results.NotFound() : Results.Ok(sites);
    }
}
