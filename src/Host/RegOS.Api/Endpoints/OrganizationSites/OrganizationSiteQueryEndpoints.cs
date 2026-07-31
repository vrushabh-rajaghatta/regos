using RegOS.Organization.Application.Queries.Sites.GetOrganizationSite;
using RegOS.Organization.Application.Queries.Sites.ListOrganizationSites;
using RegOS.Organization.Application.Queries.Sites.SiteDirectory;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Api.Endpoints.OrganizationSites;

public static class OrganizationSiteQueryEndpoints
{
    public static IEndpointRouteBuilder MapGetOrganizationSite(
        this IEndpointRouteBuilder app)
    {
        // Flat and canonical: a site is a root, and it has one URL whether you
        // arrived from its organization or from the directory.
        app.MapGet("/organization-sites/{siteId:guid}", async (
                Guid siteId,
                GetOrganizationSiteHandler handler,
                CancellationToken cancellationToken) =>
            {
                var site = await handler.HandleAsync(
                    new OrganizationSiteId(siteId), cancellationToken);

                return site is null ? Results.NotFound() : Results.Ok(site);
            })
            .WithName("GetOrganizationSite")
            .WithSummary("A single site")
            .WithTags("Organization Sites");

        return app;
    }

    public static IEndpointRouteBuilder MapListOrganizationSites(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/organizations/{organizationId:guid}/sites", async (
                Guid organizationId,
                ListOrganizationSitesHandler handler,
                CancellationToken cancellationToken) =>
            {
                var sites = await handler.HandleAsync(
                    new OrganizationId(organizationId), cancellationToken);

                return sites is null ? Results.NotFound() : Results.Ok(sites);
            })
            .WithName("ListOrganizationSites")
            .WithSummary("The sites one organization operates")
            .WithTags("Organization Sites");

        return app;
    }

    public static IEndpointRouteBuilder MapSiteDirectory(
        this IEndpointRouteBuilder app)
    {
        // "Which manufacturing sites do we have in India?" — across the
        // tenant's whole registry. Both filters optional; neither defaulted.
        app.MapGet("/organization-sites", async (
                Guid? countryId,
                OrganizationSiteType? type,
                SiteDirectoryHandler handler,
                CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(
                    countryId is { } id ? new CountryId(id) : null,
                    type,
                    cancellationToken)))
            .WithName("SiteDirectory")
            .WithSummary("Every site in the registry, filterable by country and type")
            .WithTags("Organization Sites");

        return app;
    }
}
