using RegOS.Organization.Application.Queries.Sites.SiteDirectory;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Api.Endpoints.OrganizationSites;

public static class SiteDirectoryEndpoint
{
    public static IEndpointRouteBuilder MapSiteDirectory(
        this IEndpointRouteBuilder app)
    {
        // "Which manufacturing sites do we have in India?" — across the
        // tenant's whole registry. Both filters optional; neither defaulted.
        app.MapGet("/api/organization-sites", HandleAsync)
            .WithName("SiteDirectory")
            .WithSummary("Every site in the registry, filterable by country and type")
            .WithTags("Organization Sites");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid? countryId,
        OrganizationSiteType? type,
        SiteDirectoryHandler handler,
        CancellationToken cancellationToken)
        => Results.Ok(await handler.HandleAsync(
            new SiteDirectoryQuery(
                countryId is { } id ? new CountryId(id) : null,
                type),
            cancellationToken));
}
