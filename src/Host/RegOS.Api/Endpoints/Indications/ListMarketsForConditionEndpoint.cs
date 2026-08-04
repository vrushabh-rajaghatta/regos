using RegOS.Labeling.Application.Queries.ListMarketsForCondition;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Indications;

/// <summary>
/// <b>"Which markets is this product approved for this condition in?"</b>
/// </summary>
/// <remarks>
/// The route reads as the question: a product, a condition, its markets. The
/// condition is a code rather than an id because no indication spans markets —
/// only its code does (EPIC-018 S006).
/// </remarks>
public static class ListMarketsForConditionEndpoint
{
    public static IEndpointRouteBuilder MapListMarketsForCondition(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/products/{globalProductId:guid}/indications/{conditionCode}/markets",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        string conditionCode,
        ListMarketsForConditionHandler handler,
        CancellationToken cancellationToken)
    {
        var markets = await handler.HandleAsync(
            new ListMarketsForConditionQuery(
                new GlobalProductId(globalProductId), conditionCode),
            cancellationToken);

        // Null is a missing product — a 404. An empty list means the product
        // exists and carries this indication nowhere, which is an answer.
        return markets is null
            ? Results.NotFound()
            : Results.Ok(markets);
    }
}
