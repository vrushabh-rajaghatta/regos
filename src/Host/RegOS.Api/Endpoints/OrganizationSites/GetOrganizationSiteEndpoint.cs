using RegOS.Organization.Application.Queries.Sites.GetOrganizationSite;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;

namespace RegOS.Api.Endpoints.OrganizationSites;

public static class GetOrganizationSiteEndpoint
{
    public static IEndpointRouteBuilder MapGetOrganizationSite(
        this IEndpointRouteBuilder app)
    {
        // Flat and canonical: a site is a root, and it has one URL whether you
        // arrived from its organization or from the directory.
        app.MapGet("/api/organization-sites/{siteId:guid}", HandleAsync)
            .WithName("GetOrganizationSite")
            .WithSummary("A single site")
            .WithTags("Organization Sites");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid siteId,
        GetOrganizationSiteHandler handler,
        CancellationToken cancellationToken)
    {
        var site = await handler.HandleAsync(
            new GetOrganizationSiteQuery(new OrganizationSiteId(siteId)),
            cancellationToken);

        return site is null ? Results.NotFound() : Results.Ok(site);
    }
}
