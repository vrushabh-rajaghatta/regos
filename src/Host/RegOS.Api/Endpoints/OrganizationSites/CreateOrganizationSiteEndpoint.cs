using RegOS.Organization.Application.Commands.CreateOrganizationSite;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;

namespace RegOS.Api.Endpoints.OrganizationSites;

public static class CreateOrganizationSiteEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrganizationSite(
        this IEndpointRouteBuilder app)
    {
        // Organization-scoped: a site is always somewhere a company operates,
        // and the route says so.
        app.MapPost("/api/organizations/{organizationId:guid}/sites", HandleAsync)
            .WithName("CreateOrganizationSite")
            .WithSummary("Record a site an organization operates")
            .WithTags("Organization Sites");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid organizationId,
        CreateOrganizationSiteRequest request,
        CreateOrganizationSiteHandler handler,
        CancellationToken cancellationToken)
    {
        var siteId = await handler.HandleAsync(
            new CreateOrganizationSiteCommand(
                new OrganizationId(organizationId),
                request.Name,
                request.Type,
                new CountryId(request.CountryId),
                request.StatusDate,
                request.NameNativeLanguage,
                request.AddressLine1,
                request.AddressLine2,
                request.AddressLine3,
                request.City,
                request.StateProvince,
                request.PostalCode,
                request.Email,
                request.Phone,
                [.. (request.Identifiers ?? []).Select(x =>
                    new SiteIdentifierInput(
                        new IdentifierSchemeId(x.SchemeId), x.Value))]),
            cancellationToken);

        return Results.Created(
            $"/api/organization-sites/{siteId.Value}",
            new CreateOrganizationSiteResponse(siteId.Value));
    }
}

/// <param name="StatusDate">The business date the site opened.</param>
/// <param name="Identifiers">
/// Zero or more registry identifiers — a US plant routinely has both an FEI and
/// a DUNS number.
/// </param>
public sealed record CreateOrganizationSiteRequest(
    string Name,
    OrganizationSiteType Type,
    Guid CountryId,
    DateOnly StatusDate,
    string? NameNativeLanguage = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    string? AddressLine3 = null,
    string? City = null,
    string? StateProvince = null,
    string? PostalCode = null,
    string? Email = null,
    string? Phone = null,
    IReadOnlyList<SiteIdentifierRequest>? Identifiers = null);

public sealed record SiteIdentifierRequest(Guid SchemeId, string Value);

public sealed record CreateOrganizationSiteResponse(Guid Id);
