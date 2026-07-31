using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

public static class ListRegulatoryApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapListRegulatoryApplications(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/products/{globalProductId:guid}/applications",
            HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        ListRegulatoryApplicationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GlobalProductId(globalProductId),
            cancellationToken);

        return Results.Ok(result.Applications);
    }
}
