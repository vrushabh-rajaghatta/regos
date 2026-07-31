using RegOS.Product.Domain.Product;
using RegOS.Registration.Application.Queries.ListProductRegistrations;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListProductRegistrationsEndpoint
{
    public static IEndpointRouteBuilder MapListProductRegistrations(
        this IEndpointRouteBuilder app)
    {
        // "Where is this product registered?" — half the portfolio question;
        // the other half is scoped by country.
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
