using RegOS.Product.Application.Commands.CreateMedicinalProduct;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class CreateMedicinalProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateMedicinalProduct(
        this IEndpointRouteBuilder app)
    {
        // Product-scoped: a market-local product always localises a global one,
        // and the route says so. The country is in the body because it is a
        // choice, not an address.
        app.MapPost(
            "/api/products/{globalProductId:guid}/medicinal-products",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        CreateMedicinalProductRequest request,
        CreateMedicinalProductHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateMedicinalProductCommand(
                new GlobalProductId(globalProductId),
                new CountryId(request.CountryId),
                request.StatusDate),
            cancellationToken);

        return Results.Created(
            $"/api/products/{globalProductId}/medicinal-products/{result.Id.Value}",
            new CreateMedicinalProductResponse(result.Id.Value));
    }
}
