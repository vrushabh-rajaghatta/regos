using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

public static class ListRegulatoryApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapListRegulatoryApplications(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/products/{productId:guid}/applications",
            HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        ListRegulatoryApplicationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ProductId(productId),
            cancellationToken);

        return Results.Ok(result.Applications);
    }
}
