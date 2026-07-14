using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Application.Queries.GetRegulatoryApplication;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

public static class GetRegulatoryApplicationEndpoint
{
    public static IEndpointRouteBuilder MapGetRegulatoryApplication(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/products/{productId:guid}/applications/{applicationId:guid}",
            HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        Guid applicationId,
        GetRegulatoryApplicationHandler handler,
        CancellationToken cancellationToken)
    {
        var application = await handler.HandleAsync(
            new ProductId(productId),
            new RegulatoryApplicationId(applicationId),
            cancellationToken);

        return application is null
            ? Results.NotFound()
            : Results.Ok(application);
    }
}
