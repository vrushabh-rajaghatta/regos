using RegOS.Product.Domain.Product;
using RegOS.Registration.Application.Queries.ListProductRegistrations;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListProductRegistrationsEndpoint
{
    public static IEndpointRouteBuilder MapListProductRegistrations(
        this IEndpointRouteBuilder app)
    {
        // "Where is this product registered?" — half the portfolio question.
        // The other half, by market, arrives in STORY-003.
        app.MapGet("/api/products/{productId:guid}/registrations", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        ListProductRegistrationsHandler handler,
        CancellationToken cancellationToken)
    {
        var registrations = await handler.HandleAsync(
            new ProductId(productId),
            cancellationToken);

        return registrations is null
            ? Results.NotFound()
            : Results.Ok(registrations);
    }
}
